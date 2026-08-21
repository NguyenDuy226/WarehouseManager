using System;
using System.Collections.Generic;
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
        public DateTime CreatedAt { get; set; }
        public List<DocumentLineDTO> Lines { get; set; } = new();
    }

    public class DocumentLineDTO
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateDocumentDTO
    {
        public DocumentType Type { get; set; }
        public Guid WarehouseId { get; set; }
        public List<DocumentLineCreateDTO> Lines { get; set; } = new();
    }

    public class DocumentLineCreateDTO
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateDocumentDTO
    {
        public Guid WarehouseId { get; set; }
        public List<DocumentLineCreateDTO> Lines { get; set; } = new();
    }

}