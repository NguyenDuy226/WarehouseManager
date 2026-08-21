using Warehouse.Application.DTO.Categories;

namespace Warehouse.Application.Interfaces
{
    public interface IMaterialCategoryService : IBaseService<MaterialCategoryDto, CreateMaterialCategoryDto, UpdateMaterialCategoryDto>
    {
        
    }
}