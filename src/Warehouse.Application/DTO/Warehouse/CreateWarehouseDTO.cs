using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Application.DTO.Warehouse
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public EntityStatus Status { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
    }

    public class UpdateWarehouseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }
}