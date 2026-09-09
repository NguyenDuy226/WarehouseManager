using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Application.Services
{
    public class WarehouseDetailService : IWarehouseDetailService
    {
        private readonly WarehouseDbContext _context;

        public WarehouseDetailService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<WarehouseDetailDTO>> GetDetailAsync(WarehouseDetailRequest request)
        {
           
            var query = _context.StockBalances
                .AsNoTracking()
                .Where(s => s.WarehouseId == request.WarehouseId && s.TotalQuantity > 0)
                .Join(_context.Materials, sb => sb.MaterialId, m => m.Id, (sb, m) => new { StockBalance = sb, Material = m })
                .Join(_context.MaterialCategories, x => x.Material.CategoryId, c => c.Id, (x, c) => new { x.StockBalance, x.Material, Category = c })
                .LeftJoin(_context.UnitOfMeasures, x => x.Material.UnitOfMeasureId, u => u.Id, (x, u) => new { x.StockBalance, x.Material, x.Category, Unit = u });
                
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(s => s.Material != null && 
                    (s.Material.Code.ToLower().Contains(keyword) || s.Material.Name.ToLower().Contains(keyword)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.Material.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new WarehouseDetailDTO
                {
                    Id = s.StockBalance.Id,
                    MaterialId = s.Material.Id,
                    Quantity = s.StockBalance.TotalQuantity, 
                    MaterialCode = s.Material!.Code,
                    MaterialName = s.Material!.Name,
                    MininumStock = s.Material!.MininumStock,
                    CategoryName = s.Material!.Category != null ? s.Material!.Category.Name : null,
                    UnitOfMeasureName = s.Material!.UnitOfMeasure != null ? s.Material!.UnitOfMeasure.Name : null
                })
                .ToListAsync();

            return new PagedResult<WarehouseDetailDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}