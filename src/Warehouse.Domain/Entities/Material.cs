
using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Domain.Entities
{
    public class Material
    {
        public Guid Id{ get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public MaterialCategory? Category { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        public UnitOfMeasure? UnitOfMeasure { get; set; }
        public string UnitOfMeasureId { get; set; } = string.Empty;
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
        public bool IsRemoved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}