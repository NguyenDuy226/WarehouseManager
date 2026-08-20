using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Application.DTO.Categories
{
    public class MaterialCategoryDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }

    public class CreateMaterialCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateMaterialCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }
}