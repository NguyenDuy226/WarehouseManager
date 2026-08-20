namespace Warehouse.Application.DTO.User;

public class WarehousesToUserRequestDTO
{
    public string WarehouseId { get; set; } = string.Empty;
}
public class UsersToWarehouseDTO
{
    public Guid UserId { get; set; }
}