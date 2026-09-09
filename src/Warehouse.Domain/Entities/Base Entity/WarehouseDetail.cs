// using Warehouse.Domain.Inventory.Enums;

// namespace Warehouse.Domain.Entities
// {
//     public class WarehouseDetail
//     {
//         //key
//         public Guid Id { get; set; }   
//         public Guid WarehouseId { get; set; }
//         public WarehouseEntity? Warehouse { get; set; }
//         public Guid MaterialId { get; set; }
//         public Material Material { get; set; } = null!;
//         //detail
//         public decimal Quantity { get; set; }
//         public decimal ReservedQuantity { get; set; } = 0;
//         public EntityStatus Status { get; set; }
//         public bool IsRemoved { get; set; } = false;
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//         public DateTime? LastUpdatedAt { get; set; }
//     }
// }