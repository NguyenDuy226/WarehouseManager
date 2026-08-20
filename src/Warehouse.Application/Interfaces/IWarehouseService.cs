using Warehouse.Application.DTO.Warehouse;

namespace Warehouse.Application.Interfaces
{
    public interface IWarehouseService : IBaseService<WarehouseDto, CreateWarehouseDto, UpdateWarehouseDto>
    {

    }
}