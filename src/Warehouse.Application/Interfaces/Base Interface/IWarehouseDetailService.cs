using System.Threading.Tasks;
using Warehouse.Application.DTO;
using Warehouse.Application.DTO.Paging;

namespace Warehouse.Application.Interfaces
{
    public interface IWarehouseDetailService
    {
        Task<PagedResult<WarehouseDetailDTO>> GetDetailAsync(WarehouseDetailRequest request);
    }
}