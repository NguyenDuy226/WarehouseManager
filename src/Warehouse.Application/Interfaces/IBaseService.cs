using Warehouse.Application.DTO.Paging;

namespace Warehouse.Application.Interfaces
{
    public interface IBaseService<TDto, TCreate, TUpdate> where TDto : class
    {
        Task<PagedResult<TDto>> GetAllAsync(PagingRequest request);
        Task<TDto?> GetByIdAsync(Guid id);
        Task<TDto> CreateAsync(TCreate dto);
        Task<bool> UpdateAsync(Guid id, TUpdate dto);
        Task<bool> DeleteAsync(Guid id);
    }
}