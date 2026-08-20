using Warehouse.Application.DTO.Suppliers;

namespace Warehouse.Application.Interfaces
{
    public interface ISupplierService : IBaseService<SupplierDto, CreateSupplierDto, UpdateSupplierDto>
    {
        
    }
}