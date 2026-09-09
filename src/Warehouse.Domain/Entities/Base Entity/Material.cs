
using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Domain.Entities
{
    public class Material
    {
        public Guid Id{ get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public MaterialCategory? Category { get; set; }
        public Guid CategoryId { get; set; } 
        public UnitOfMeasure? UnitOfMeasure { get; set; }
        public Guid UnitOfMeasureId { get; set; } 
        public decimal RefPrice { get; set; }
        public decimal MininumStock { get; set; }
        public EntityStatus Status { get; set; }
        public bool IsRemoved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();

    }
}