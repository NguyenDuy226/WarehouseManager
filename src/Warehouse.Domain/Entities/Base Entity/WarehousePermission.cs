namespace Warehouse.Domain.Entities;

public class WarehousePermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid WarehouseId { get; set; } 
    public AppUser? User { get; set; }
    public WarehouseEntity? Warehouse { get; set; }
    public bool IsRemoved { get; set; } = false;

}