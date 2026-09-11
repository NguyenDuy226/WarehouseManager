using System;
using System.Collections.Generic;
using System.Linq;

namespace Warehouse.Domain.Entities
{
    //base class
    public abstract class StockDocument
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public string Code { get; protected set; } = string.Empty;
        public Guid WarehouseId { get; protected set; } 
        public virtual WarehouseEntity? Warehouse { get; protected set; }
        public DocumentType Type { get; protected set; }
        public DocumentStatus Status { get; protected set; } = DocumentStatus.DRAFT;
        public string CreatedBy { get; protected set; }
        public DateTime DocumentDate { get; protected set; }
        public string Reason { get; protected set; } = string.Empty;
        public string Note { get; protected set; } = string.Empty;
        public List<StockDocumentLine> Lines { get; protected set; } = [];

        protected StockDocument(DocumentType type, string createdBy, Guid warehouseId)
        {
            Type = type;
            CreatedBy = createdBy;
            WarehouseId = warehouseId;
            Status = DocumentStatus.DRAFT;
            DocumentDate = DateTime.UtcNow;
            
        }

        public void SetDocumentDetails(DateTime documentDate, string reason, string note)
        {
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
                throw new InvalidOperationException("only DRAFT and REJECTED can be update");

            DocumentDate = documentDate;
            Reason = reason ?? string.Empty;
            Note = note ?? string.Empty;
        }

        public void AddOrUpdateLine(Guid materialId, decimal quantity, decimal price)
        {
            if(Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
                throw new InvalidOperationException("only DRAFT and REJECTED can be update");

            if (quantity <= 0)
                throw new ArgumentException("quantity have to > 0");
            
            var existingLine = Lines.FirstOrDefault(l => l.MaterialId == materialId && !l.IsRemoved);
            if (existingLine != null)
            {
                existingLine.Update(materialId, quantity, price);
            }
            else
            {
                Lines.Add(new StockDocumentLine(this.Id, materialId, quantity, price));
            }
        }

        public void SubmitForApproval()
        {
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
                throw new InvalidOperationException("only DRAFT and REJECT can be approve");                
            
            if (!Lines.Any(l => !l.IsRemoved))
                throw new InvalidOperationException("doc have at least one line");
            
            Status = DocumentStatus.PENDING_APPROVAL;
        }

        public void Approve(string approverId, string approverRole) 
        {
            if (Status != DocumentStatus.PENDING_APPROVAL)
                throw new InvalidOperationException("doc is not in PENDING_APPROVAL state");
            
            if (approverId == this.CreatedBy && approverRole != "SYSTEM_ADMIN" && approverRole != "WAREHOUSE_MANAGER"  && approverRole != "APPROVER")
                throw new UnauthorizedAccessException("only MANAGER and ADMIN can approve");
            
            Status = DocumentStatus.POSTED;
        }

        public void Reject(string rejectorId, string reason, string rejectorRole)
        {
            if (Status != DocumentStatus.PENDING_APPROVAL) throw new InvalidOperationException("doc is not in PENDING_APPROVAL state");
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("have to fill reason");
            
            if (rejectorId == this.CreatedBy && rejectorRole != "SYSTEM_ADMIN" && rejectorRole != "WAREHOUSE_MANAGER"  && rejectorRole != "APPROVER")
                throw new UnauthorizedAccessException("only MANAGER and ADMIN can approve");
            

            Status = DocumentStatus.REJECTED;
        }

        public void Cancel(string cancelerId, string cancelerRole)
        {
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.PENDING_APPROVAL)
                throw new InvalidOperationException("only DRAFT and PENDING_APPROVAL can be deleted");
            if (cancelerId == this.CreatedBy && cancelerRole != "SYSTEM_ADMIN" && cancelerRole != "WAREHOUSE_MANAGER" && cancelerRole != "APPROVER")
                throw new UnauthorizedAccessException("only MANAGER and ADMIN can approve");
            

            Status = DocumentStatus.CANCELED;
        }
        
        public virtual StockDocument CreateReversalDocument(string createdBy, string reversalCode)
        {
            if (Status != DocumentStatus.POSTED)
                throw new InvalidOperationException("only POSTED doc can be reversal");
            var reversalDoc = new ReversalDocument(createdBy, this.WarehouseId, this.Id);
            reversalDoc.SetCode(reversalCode);
            foreach (var line in Lines.Where(l => !l.IsRemoved)) 
            {
                reversalDoc.Lines.Add(line.CloneForReversal(reversalDoc.Id));
            }

            return reversalDoc;
        }
    
        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("code cant be null", nameof(code));
            
            if (!string.IsNullOrWhiteSpace(Code))
                throw new InvalidOperationException("code is existed and cant be change");
            
            Code = code;
        }
    
    }

    //RECEIPT
    public class ReceiptDocument : StockDocument
    {
        public Guid? SupplierId { get; private set; }
        public string ExternalDocumentNo { get; private set; } = string.Empty;
        public virtual Supplier? Supplier { get; private set; }
        public ReceiptDocument(string createdBy, Guid warehouseId) 
            : base(DocumentType.RECEIPT, createdBy, warehouseId)
        {
        }

        public void SetReceiptDetails(Guid? supplierId, string externalDocumentNo, DateTime documentDate, string reason, string note)
        {
            base.SetDocumentDetails(documentDate, reason, note);
            
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
                throw new InvalidOperationException("only DRAFT and REJECTED can be update");

            SupplierId = supplierId;
            ExternalDocumentNo = externalDocumentNo ?? string.Empty;
        }
    }

    //ISSUE
    public class IssueDocument : StockDocument
    {
        public string Receiver { get; private set; } = string.Empty;
        public string ExternalDocumentNo { get; private set; } = string.Empty;

        public IssueDocument(string createdBy, Guid warehouseId) : base(DocumentType.ISSUE, createdBy, warehouseId)
        {
        }

        public void SetIssueDetails(string receiver, string externalDocumentNo, DateTime documentDate, string reason, string note)
        {
            base.SetDocumentDetails(documentDate, reason, note);
            
            if (Status != DocumentStatus.DRAFT && Status != DocumentStatus.REJECTED)
                throw new InvalidOperationException("only DRAFT and REJECTED can be update");

            Receiver = receiver ?? string.Empty;
            ExternalDocumentNo = externalDocumentNo ?? string.Empty;
        }
    }
    
    //TRANSFER
    public class TransferDocument : StockDocument
    {
        public Guid ToWarehouseId { get; private set; }
        public virtual WarehouseEntity? ToWarehouse { get; private set; }
        public TransferDocument(string createdBy, Guid warehouseId, Guid toWarehouseId) : base(DocumentType.TRANSFER, createdBy, warehouseId)
        {
            if (toWarehouseId == Guid.Empty || toWarehouseId == warehouseId)
                throw new ArgumentException("invalid target warehouse");
            ToWarehouseId = toWarehouseId;
        }
    }
    
    //ADJUST
    public class AdjustmentDocument : StockDocument
    {
        public AdjustmentDocument(string createdBy, Guid warehouseId) : base(DocumentType.ADJUSTMENT, createdBy, warehouseId)
        {
        }
    }
    
    //REVERSAL
    public class ReversalDocument : StockDocument
    {
        public Guid OriginalDocumentId { get; private set; }

        public ReversalDocument(string createdBy, Guid warehouseId, Guid originalDocumentId) 
            : base(DocumentType.REVERSAL, createdBy, warehouseId)
        {
            OriginalDocumentId = originalDocumentId;
        }
    }

    //OPENING
    public class OpeningDocument : StockDocument
    {
        public OpeningDocument(string createdBy, Guid warehouseId) 
            : base(DocumentType.OPENING, createdBy, warehouseId)
        {
        }
    }

    //LINE
    public class StockDocumentLine
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public Guid MaterialId { get; private set; }
        public decimal Quantity { get; private set; } 
        public decimal UnitPrice { get; private set; }       
        public bool IsRemoved { get; private set; } = false;
        public StockDocument? Document { get; private set; }
        public virtual Material? Material { get; private set; }

        public decimal TotalAmount => Math.Round(Quantity * UnitPrice, 4);

        public StockDocumentLine(Guid documentId, Guid materialId, decimal quantity, decimal unitPrice)
        {
            ValidInput(quantity, unitPrice);
            Id = Guid.NewGuid();
            DocumentId = documentId;
            MaterialId = materialId;
            Quantity = Math.Round(quantity, 4); 
            UnitPrice = Math.Round(unitPrice, 4);
        }
        
        public void Update(Guid materialId, decimal quantity, decimal unitPrice)
        {
            MaterialId = materialId;
            ValidInput(quantity, unitPrice);
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
        
        private void ValidInput(decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0) throw new ArgumentException("quantity have to > 0", nameof(quantity));
            if (unitPrice < 0) throw new ArgumentException("unit price have to >= 0", nameof(unitPrice));
        }
    }
        
    //MOVEMENT     
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

    //BALANCE
    public class StockBalance
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
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
            // MovingAveragePrice = 0;
        }

        public void UpdateBalance(decimal newQuantity, decimal newValue, decimal newMap)
        {
            if (newQuantity < 0) throw new InvalidOperationException("total quantity cannot be negative");

            TotalQuantity = Math.Round(newQuantity, 4);
            TotalValue = Math.Round(newValue, 4);
            // MovingAveragePrice = Math.Round(newMap, 4);
        }
    }    

    //HISTORY
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