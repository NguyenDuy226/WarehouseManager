using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Warehouse.Domain.Entities;

namespace Warehouse.Application.DTO
{
    public class DocumentDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public DocumentStatus Status { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty; //id
        public string CreatorName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        //receipt
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; } 
        public string? ExternalDocumentNo { get; set; }
        //issue
        public string? Receiver { get; set; }
        //transfer
        public Guid? ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public List<DocumentLineDTO> Lines{ get; set; } = new();
    }
    public class DocumentLineDTO
    {
        public Guid? Id { get; set; }
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

    }

    public class DocumentLineCreateDTO
    {
        public Guid? Id { get; set; }
        [Required]
        public Guid MaterialId { get; set; }

        [Required]
        [Range(0.0001, (double)decimal.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    //create dto
    public abstract class CreateDocumentBaseDTO
    {
        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public DateTime DocumentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<DocumentLineCreateDTO> Lines { get; set; } = new();
    }

    public class CreateReceiptDTO : CreateDocumentBaseDTO
    {
        public Guid? SupplierId { get; set; }

        [MaxLength(100)]
        public string ExternalDocumentNo { get; set; } = string.Empty;
    }

    public class CreateIssueDTO : CreateDocumentBaseDTO
    {
        [MaxLength(255)]
        public string Receiver { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ExternalDocumentNo { get; set; } = string.Empty;
    }

    public class CreateTransferDTO : CreateDocumentBaseDTO
    {
        [Required]
        public Guid ToWarehouseId { get; set; }
    }

    public class CreateAdjustmentDTO : CreateDocumentBaseDTO
    {
    }

    public class CreateOpeningDTO : CreateDocumentBaseDTO
    {
    }

    public class CreateReversalDTO : CreateDocumentBaseDTO
    {
        [Required]
        public Guid OriginalDocumentId { get; set; }
    }

    //update dto
    public abstract class UpdateDocumentBaseDTO
    {
        [Required]
        public DateTime DocumentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<DocumentLineCreateDTO> Lines { get; set; } = new();
    }

    public class UpdateReceiptDTO : UpdateDocumentBaseDTO
    {
        public Guid? SupplierId { get; set; }

        [MaxLength(100)]
        public string ExternalDocumentNo { get; set; } = string.Empty;
    }

    public class UpdateIssueDTO : UpdateDocumentBaseDTO
    {
        [MaxLength(255)]
        public string Receiver { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ExternalDocumentNo { get; set; } = string.Empty;
    }

    public class UpdateTransferDTO : UpdateDocumentBaseDTO
    {
        [Required]
        public Guid ToWarehouseId { get; set; }
    }

    public class UpdateAdjustmentDTO : UpdateDocumentBaseDTO
    {
    }

    public class UpdateOpeningDTO : UpdateDocumentBaseDTO
    {
    }

    
    public class RejectDocumentRequest
    {
        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;
    }
}