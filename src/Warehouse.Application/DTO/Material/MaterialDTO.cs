using Warehouse.Domain.Inventory.Enums; 

namespace Warehouse.Application.DTO.Materials
{
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string UnitOfMeasureId { get; set; } = string.Empty;
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
    }

    public class CreateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string UnitOfMeasureId { get; set; } = string.Empty;
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
    }

    public class UpdateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string UnitOfMeasureId { get; set; } = string.Empty;
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
    }
}