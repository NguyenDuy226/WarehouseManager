using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Application.DTO.Suppliers
{
    public class SupplierDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Addres { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Addres { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }

    public class UpdateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Addres { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }
}