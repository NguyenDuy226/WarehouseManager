using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;
using Warehouse.Infrastructure.Services;

namespace Warehouse.Application.Services
{
    public class OpeningDocumentService : IOpeningDocumentService
    {
        private readonly WarehouseDbContext _context;
        private readonly IBaseDocumentService _baseService;
        private readonly CodeGenerator _codeGenerator;

        public OpeningDocumentService(WarehouseDbContext context, IBaseDocumentService baseService, CodeGenerator codeGenerator)
        {
            _context = context;
            _baseService = baseService;
            _codeGenerator = codeGenerator;
        }

        public async Task<Guid> CreateOpeningAsync(CreateOpeningRequest request, string userId)
        {
            //require: dont have any movement
            if (request.Lines == null || !request.Lines.Any())
            {
                throw new ArgumentException("document have to have at least 1 line");
            }
            var materialIds = request.Lines.Select(l => l.MaterialId).ToList();
            bool hasMovement = await _context.Set<StockMovement>()
                .AnyAsync(m => m.WarehouseId == request.WarehouseId && materialIds.Contains(m.MaterialId));
            if (hasMovement)
            {
                throw new InvalidOperationException("material have movement, cant create opening");
            }
            var warehouse = await _context.WarehouseEntities.FindAsync(request.WarehouseId);
            if(warehouse == null)
            {
                throw new InvalidOperationException("warehouse not found");
            }
            //create header
            string code = await _codeGenerator.GenerateDocumentCode(DocumentType.OPENING, warehouse.Code, 6);
            var document = new StockDocument(DocumentType.OPENING, userId, request.WarehouseId);
            document.SetCode(code );
            //add line
            foreach (var line in request.Lines)
            {
                document.AddOrUpdateLine(line.MaterialId, line.Quantity, line.UnitPrice);
            }

            _context.Set<StockDocument>().Add(document);
            await _context.SaveChangesAsync();
            return document.Id;
        }

        public async Task ApproveOpeningAsync(Guid documentId, Guid approverId, string approverRole)
        {
            var document = await _context.Set<StockDocument>()
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("document not found");
            //DRAFT -> PENDING_APPROVAL
            if (document.Status == DocumentStatus.DRAFT)
            {
                document.SubmitForApproval();
            }
            //PENDING_APPROVAL -> POSTED
            document.Approve(approverId.ToString(), approverRole);

            var history = new ApprovalHistory(
                documentId: document.Id, 
                userId: approverId, 
                action: "APPROVE",
                note: "Duyệt chứng từ số dư đầu kỳ"
            );
            _context.Set<ApprovalHistory>().Add(history);

            await _baseService.PostDocumentAsync(document);
        }
    
    }
}