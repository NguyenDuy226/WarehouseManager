using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Application.DTO.User
{
    public class UserWarehouseDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }
}