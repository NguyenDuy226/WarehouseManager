namespace Warehouse.Application.DTO
{
    public class PermissionDTO{
        public Guid UserId { get; set; } 
        public string WarehouseId { get; set; } = string.Empty;
    }
}