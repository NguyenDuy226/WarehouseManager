namespace Warehouse.Application.DTO.User
{
    public class UpdateUserRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}