using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Domain.Entities
{
    public class MaterialCategory
    {
        public Guid Id { get; set; } 
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EntityStatus  Status { get; set; }
        public ICollection<Material> Materials {get; set;} = [];
        public bool IsRemoved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}