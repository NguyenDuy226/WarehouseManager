using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.UnitOfMeasure;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.UnitsOfMeasure
{
    public class UnitOfMeasureService : IUnitOfMeasureService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;

        public UnitOfMeasureService(WarehouseDbContext context, CodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<PagedResult<UnitOfMeasureDto>> GetAllAsync(PagingRequest request)
        {
            var query = _context.UnitOfMeasures.AsQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UnitOfMeasureDto
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Status = u.Status
                })
                .ToListAsync();

            return new PagedResult<UnitOfMeasureDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<UnitOfMeasureDto?> GetByIdAsync(Guid id)
        {
            var u = await _context.UnitOfMeasures.FindAsync(id);
            if (u == null) return null;

            return new UnitOfMeasureDto
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Status = u.Status
            };
        }

        public async Task<UnitOfMeasureDto> CreateAsync(CreateUnitOfMeasureDto dto)
        {
            var item = new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<UnitOfMeasure>(),
                Name = dto.Name,
                Status = EntityStatus.Active
            };

            await _context.UnitOfMeasures.AddAsync(item);
            await _context.SaveChangesAsync();

            return new UnitOfMeasureDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                Status = item.Status
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUnitOfMeasureDto dto)
        {
            var item = await _context.UnitOfMeasures.FindAsync(id);
            if (item == null) return false;

            item.Name = dto.Name;
            item.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.UnitOfMeasures.FindAsync(id);
            if (item == null) return false;
            item.IsRemoved = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}