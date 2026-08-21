using System;
using System.Collections.Generic;

namespace Warehouse.Domain.Entities
{
    public class StockDocument{
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; private set; } = string.Empty;
        public Guid WarehouseId { get; private set; } 
        public Guid? ToWarehouseId { get; private set; } 
        public DocumentType Type { get; private set; }
        public DocumentStatus Status { get; private set; } = DocumentStatus.DRAFT;
        public string CreatedBy { get; private set; }
        public List<StockDocumentLine> Lines { get; private set; } = [];
        public StockDocument(DocumentType type, string createdBy, Guid warehouseId, Guid? toWarehouseId = null){
            if (type == DocumentType.TRANSFER && (toWarehouseId == null || toWarehouseId == Guid.Empty))
            {
                throw new ArgumentException("TRANSFER have to have target warehouse");
            }
            Type = type;
            CreatedBy = createdBy;
            Status = DocumentStatus.DRAFT;
            WarehouseId = warehouseId;
            ToWarehouseId = toWarehouseId;
        }

        public void AddOrUpdateLine(Guid materialId, decimal quantity, decimal price){
            if(Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
            {
                throw new InvalidOperationException("only DRAFT and REJECTED can be update");
            }
            if (quantity <= 0)
            {
                throw new ArgumentException("quantity have to > 0");
            }
            var existingLine = Lines.FirstOrDefault(l => l.MaterialId == materialId);
            if (existingLine != null){
                existingLine.Update(quantity, price);
            }
            else{
                Lines.Add(new StockDocumentLine(this.Id, materialId, quantity, price));
            }
        }

        public void Cancel(){
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
            {
                throw new InvalidOperationException("only DRAFT and REJECTED can be deleted");
            }
            Status = DocumentStatus.CANCELED;
        }
        
        public void SubmitForApproval(){
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
            {
                throw new InvalidOperationException("only DRAFT and REJECT can be approve");                
            }
            if (!Lines.Any())
            {
                throw new InvalidOperationException("doc have at least one line");
            }
            Status = DocumentStatus.PENDING_APPROVAL;
        }

        public void Approve(string approverId, string approverRole) {
            if (Status != DocumentStatus.PENDING_APPROVAL)
            {
                throw new InvalidOperationException("doc is not in PENDING_APPROVAL state");
            }
            if (approverId == this.CreatedBy && approverRole != "SYSTEM_ADMIN" && approverRole != "WAREHOUSE_MANAGER")
            {
                throw new UnauthorizedAccessException("only MANAGER and ADMIN can approve");
            }
            Status = DocumentStatus.POSTED;
        }

        public void Reject(string rejectorId, string reason){
            if (Status != DocumentStatus.PENDING_APPROVAL)
            {
                throw new InvalidOperationException("doc is not in PENDING_APPROVAL state");
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("have to fill reason");
            }
            Status = DocumentStatus.REJECTED;
        }

        public StockDocument CreateReversalDocument(string createdBy, string reversalCode)
        {
            if (Status != DocumentStatus.POSTED)
            {
                throw new InvalidOperationException("only POSTED doc can be reversal");
            }
            var reversalDoc = new StockDocument(DocumentType.REVERSAL, createdBy, this.WarehouseId, this.ToWarehouseId);
            
            foreach (var line in Lines) 
            {
                reversalDoc.Lines.Add(line.CloneForReversal(reversalDoc.Id));
            }

            return reversalDoc;
        }
    
        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("code cant be null", nameof(code));
            }
            if (!string.IsNullOrWhiteSpace(Code))
            {
                throw new InvalidOperationException("code is existed and cant be changed");
            }
            Code = code;
        }
            }

    public class StockDocumentLine
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public Guid MaterialId { get; private set; }
        public decimal Quantity { get; private set; } 
        public decimal UnitPrice { get; private set; } 
        
        public bool IsRemoved { get; private set; } = false;
        public StockDocument? Document { get; private set; }

        public decimal TotalAmount => Math.Round(Quantity * UnitPrice, 4);

        public StockDocumentLine(Guid documentId, Guid materialId, decimal quantity, decimal unitPrice)
        {
            ValidateInputs(quantity, unitPrice);
            Id = Guid.NewGuid();
            DocumentId = documentId;
            MaterialId = materialId;
            Quantity = Math.Round(quantity, 4); 
            UnitPrice = Math.Round(unitPrice, 4);
        }
        public void Update(decimal quantity, decimal unitPrice)
        {
            ValidateInputs(quantity, unitPrice);
            Quantity = Math.Round(quantity, 4);
            UnitPrice = Math.Round(unitPrice, 4);
            IsRemoved = false; 
        }
        public void MarkAsRemoved()
        {
            IsRemoved = true;
        }
        public void Restore()
        {
            IsRemoved = false;
        }
        public StockDocumentLine CloneForReversal(Guid reversalDocumentId)
        {
            return new StockDocumentLine(reversalDocumentId, MaterialId, Quantity, UnitPrice);
        }
        private void ValidateInputs(decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("quantity have to > 0", nameof(quantity));
            }
            if (unitPrice < 0)
            {
                throw new ArgumentException("unit of price g=have to >= 0", nameof(unitPrice));
            }
        }
    }
        
    public class StockMovement
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid WarehouseId { get; private set; }
        public Guid MaterialId { get; private set; }
        public Guid DocumentId { get; private set; }
        public decimal MovementQuantity { get; private set; } 
        public decimal UnitPrice { get; private set; } 
        public decimal BalanceAfter { get; private set; } 
        
        public DateTime MovementTime { get; private set; } = DateTime.UtcNow;
        public StockMovement(Guid warehouseId, Guid materialId, Guid documentId, decimal movementQuantity, decimal unitPrice, decimal balanceAfter)
        {
            WarehouseId = warehouseId;
            MaterialId = materialId;
            DocumentId = documentId;
            MovementQuantity = Math.Round(movementQuantity, 4);
            UnitPrice = Math.Round(unitPrice, 4);
            BalanceAfter = Math.Round(balanceAfter, 4);
        }
    }

    public class StockBalance
    {
        public Guid WarehouseId { get; private set; }
        public Guid MaterialId { get; private set; }
        public decimal TotalQuantity { get; private set; }
        public decimal TotalValue { get; private set; }
        public decimal MovingAveragePrice { get; private set; } 
        public byte[]? RowVersion { get; private set; }

        public StockBalance(Guid warehouseId, Guid materialId)
        {
            WarehouseId = warehouseId;
            MaterialId = materialId;
            TotalQuantity = 0;
            TotalValue = 0;
            MovingAveragePrice = 0;
        }

        public void UpdateBalance(decimal newQuantity, decimal newValue, decimal newMap)
        {
            if (newQuantity < 0)
            {
                throw new InvalidOperationException("Total quantity cannot be negative (BR-012)");
            }

            TotalQuantity = Math.Round(newQuantity, 4);
            TotalValue = Math.Round(newValue, 4);
            MovingAveragePrice = Math.Round(newMap, 4);
        }
    }    

    public class ApprovalHistory
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid DocumentId { get; private set; }
        public Guid UserId { get; private set; }
        public string Action { get; private set; } 
        public string Note { get; private set; } 
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
                
        public StockDocument? Document { get; private set; }

        public ApprovalHistory(Guid documentId, Guid userId, string action, string note)
        {
            DocumentId = documentId;
            UserId = userId;
            Action = action;
            Note = note ?? string.Empty;
        }
    }


}