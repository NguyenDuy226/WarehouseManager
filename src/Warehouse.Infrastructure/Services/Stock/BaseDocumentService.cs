using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Application.Services
{
    public class BaseDocumentService : IBaseDocumentService
    {
        private readonly WarehouseDbContext _context;

        public BaseDocumentService(WarehouseDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task PostDocumentAsync(StockDocument document)
        {
            if (document.Status != DocumentStatus.POSTED)
            {
                throw new InvalidOperationException("POSTED is invalid");
            }
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                foreach (var line in document.Lines)
                {
                    switch (document.Type)
                    {
                        case DocumentType.RECEIPT:
                            await MoveInAsync(document.WarehouseId, document.Id, line.MaterialId, line.Quantity, line.UnitPrice);
                            break;

                        case DocumentType.OPENING:
                            bool hasMovement = await _context.Set<StockMovement>()
                                .AnyAsync(m => m.WarehouseId == document.WarehouseId && m.MaterialId == line.MaterialId);
                            if (hasMovement)
                                throw new InvalidOperationException("Material already has movements");
                            await MoveInAsync(document.WarehouseId, document.Id, line.MaterialId, line.Quantity, line.UnitPrice);
                            break;

                        case DocumentType.ISSUE:
                            await MoveOutAsync(document.WarehouseId, document.Id, line.MaterialId, line.Quantity);
                            break;

                        case DocumentType.TRANSFER:
                            await ProcessTransferAsync(document, line);
                            break;
                        case DocumentType.ADJUSTMENT:
                            if (line.Quantity > 0)
                            {
                                var balance = await GetOrCreateBalanceAsync(document.WarehouseId, line.MaterialId);
                                decimal priceToUse = balance.TotalQuantity > 0 ? balance.MovingAveragePrice : line.UnitPrice;
                                if (priceToUse <= 0)
                                    throw new ArgumentException("UnitPrice is required");
                                await MoveInAsync(document.WarehouseId, document.Id, line.MaterialId, line.Quantity, priceToUse);
                            }
                            else
                            {
                                await MoveOutAsync(document.WarehouseId, document.Id, line.MaterialId, Math.Abs(line.Quantity));
                            }
                            break;
                            
                        case DocumentType.REVERSAL:
                            await ProcessReversalAsync(document, line);
                            break;
                        default:
                            throw new NotSupportedException($"create {document.Type} fail");
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("data has been changed, pls try again");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        //nhap kho, movement (+)
        private async Task MoveInAsync(Guid warehouseId, Guid documentId, Guid materialId, decimal quantity, decimal unitPrice)
        {
            var balance = await GetOrCreateBalanceAsync(warehouseId, materialId);
            decimal receiptValue = Math.Round(quantity * unitPrice, 4);
            decimal newQuantity = balance.TotalQuantity + quantity;
            decimal newValue = balance.TotalValue + receiptValue;
            decimal newMap = newQuantity == 0 ? 0 : Math.Round(newValue / newQuantity, 4);

            balance.UpdateBalance(newQuantity, newValue, newMap);

            var movement = new StockMovement(warehouseId, materialId, documentId, quantity, unitPrice, newQuantity);
            _context.Set<StockMovement>().Add(movement);
        }

        //xuat kho, movement (-)
        private async Task MoveOutAsync(Guid warehouseId, Guid documentId, Guid materialId, decimal quantity)
        {
            var balance = await GetOrCreateBalanceAsync(warehouseId, materialId);

            if (balance.TotalQuantity < quantity)
                throw new InvalidOperationException($"{materialId} is not enough quantity in warehouse (uantity: {balance.TotalQuantity}, request quantity: {quantity}).");

            decimal issuePrice = balance.MovingAveragePrice;
            decimal issueValue = Math.Round(quantity * issuePrice, 4);
            decimal newQuantity = balance.TotalQuantity - quantity;
            decimal newValue = balance.TotalValue - issueValue;
            decimal newMap = newQuantity == 0 ? 0 : balance.MovingAveragePrice;

            balance.UpdateBalance(newQuantity, newValue, newMap);
            var movement = new StockMovement(warehouseId, materialId, documentId, -quantity, issuePrice, newQuantity);
            _context.Set<StockMovement>().Add(movement);
        }

        //chuyen kho
        private async Task ProcessTransferAsync(StockDocument document, StockDocumentLine line)
        {
            await MoveOutAsync(document.WarehouseId, document.Id, line.MaterialId, line.Quantity);
            var sourceBalance = await _context.Set<StockBalance>()
                .FirstAsync(b => b.WarehouseId == document.WarehouseId && b.MaterialId == line.MaterialId);
            
            await MoveInAsync(document.ToWarehouseId!.Value, document.Id, line.MaterialId, line.Quantity, sourceBalance.MovingAveragePrice);
        }

        //lay ton kho neu khong co thi tao moi
        private async Task<StockBalance> GetOrCreateBalanceAsync(Guid warehouseId, Guid materialId)
        {
            var balance = await _context.Set<StockBalance>()
                .FirstOrDefaultAsync(b => b.WarehouseId == warehouseId && b.MaterialId == materialId);
            if (balance == null)
            {
                balance = new StockBalance(warehouseId, materialId);
                _context.Set<StockBalance>().Add(balance);
            }
            return balance;
        }
        
        private Task ProcessReversalAsync(StockDocument document, StockDocumentLine line)
        {
            return Task.CompletedTask;
        }
    }
}