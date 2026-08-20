using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Application.DTO.UnitOfMeasure
{
    public class UnitOfMeasureDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }

    public class CreateUnitOfMeasureDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateUnitOfMeasureDto
    {
        public string Name { get; set; } = string.Empty;
        public EntityStatus Status { get; set; }
    }
}