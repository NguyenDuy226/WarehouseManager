using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.Warehouse;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.Warehouse
{
    public class WarehouseService : IWarehouseService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;

        public WarehouseService(WarehouseDbContext context, CodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<PagedResult<WarehouseDto>> GetAllAsync(PagingRequest request)
        {
            var query = _context.WarehouseEntities.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
               var keyword = request.Keyword.Trim().ToLower(); 
                query = query.Where(x => 
                    (x.Code != null && x.Code.ToLower().Contains(keyword)) || 
                    (x.Name != null && x.Name.ToLower().Contains(keyword)) 
                );
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.ToLower() != "all")
            {
                var status = request.Status.ToLower().Trim();
                if (status == "active")
                {
                    query = query.Where(x => x.Status == EntityStatus.Active);
                }
                else if (status == "inactive")
                {
                    query = query.Where(x => x.Status == EntityStatus.Inactive);
                }
            }
           
            var totalCount = await query.CountAsync();
            var sortDirection = request.SortDirection?.ToLower() == "asc" ? "asc" : "desc";
            var sortBy = request.SortBy?.ToLower() ?? "createdat";
            query = sortBy switch
            {
                "name" => sortDirection == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                "code" => sortDirection == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                _ => sortDirection == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
            };
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new WarehouseDto
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name,
                    Address = t.Address,
                    Manager = t.Manager,
                    CreatedAt = t.CreatedAt,
                    Status = t.Status
                })
                .ToListAsync();
            return new PagedResult<WarehouseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<WarehouseDto?> GetByIdAsync(Guid id)
        {
            return await _context.WarehouseEntities
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(item => new WarehouseDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    Address = item.Address,
                    Manager = item.Manager,
                    CreatedAt = item.CreatedAt,
                    Status = item.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
        {
            var item = new WarehouseEntity
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<WarehouseEntity>(),
                Name = dto.Name,
                Address = dto.Address,
                Manager = dto.Manager,
                CreatedAt = DateTime.UtcNow
            };
            await _context.WarehouseEntities.AddAsync(item);
            await _context.SaveChangesAsync();
            return new WarehouseDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                Address = item.Address,
                Manager = item.Manager,
                CreatedAt = item.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateWarehouseDto dto)
        {
            var item = await _context.WarehouseEntities.FindAsync(id);
            if (item == null) return false;
            item.Name = dto.Name;
            item.Address = dto.Address;
            item.Manager = dto.Manager;
            item.Status = dto.Status;
            _context.WarehouseEntities.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.WarehouseEntities.FindAsync(id);
            if (item == null) return false;
            item.IsRemoved = true;
            await _context.SaveChangesAsync();
            return true;
        }


    }
}