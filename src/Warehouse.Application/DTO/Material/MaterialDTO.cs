using Warehouse.Domain.Inventory.Enums; 

namespace Warehouse.Application.DTO.Materials
{
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; } 
        public Guid UnitOfMeasureId { get; set; } 
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }

    public class CreateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; } 
        public Guid UnitOfMeasureId { get; set; } 
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
    }

    public class UpdateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; } 
        public Guid UnitOfMeasureId { get; set; }
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
    }
}