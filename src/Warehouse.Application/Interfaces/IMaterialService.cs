using Warehouse.Application.DTO.Materials;

namespace Warehouse.Application.Interfaces
{
    public interface IMaterialService : IBaseService<MaterialDto, CreateMaterialDto, UpdateMaterialDto>
    {
        
    }
}