using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Materials;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.Materials
{
    public class MaterialService : IMaterialService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;

        public MaterialService(WarehouseDbContext context, CodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<PagedResult<MaterialDto>> GetAllAsync(PagingRequest request)
        {
            var query = _context.Materials.AsQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(m => m.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    CategoryId = m.CategoryId,
                    UnitOfMeasureId = m.UnitOfMeasureId,
                    RefPrice = m.RefPrice,
                    MininumStock = m.MininumStock,
                    Status = m.Status
                })
                .ToListAsync();

            return new PagedResult<MaterialDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<MaterialDto?> GetByIdAsync(Guid id)
        {
            var m = await _context.Materials.FindAsync(id);
            if (m == null) return null;

            return new MaterialDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                CategoryId = m.CategoryId,
                UnitOfMeasureId = m.UnitOfMeasureId,
                RefPrice = m.RefPrice,
                MininumStock = m.MininumStock,
                Status = m.Status
            };
        }

        public async Task<MaterialDto> CreateAsync(CreateMaterialDto dto)
        {
            var item = new Material
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<Material>(),
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                UnitOfMeasureId = dto.UnitOfMeasureId,
                RefPrice = dto.RefPrice,
                MininumStock = dto.MininumStock,
                Status = EntityStatus.Active
            };

            await _context.Materials.AddAsync(item);
            await _context.SaveChangesAsync();

            return new MaterialDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                CategoryId = item.CategoryId,
                UnitOfMeasureId = item.UnitOfMeasureId,
                RefPrice = item.RefPrice,
                MininumStock = item.MininumStock,
                Status = item.Status
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateMaterialDto dto)
        {
            var item = await _context.Materials.FindAsync(id);
            if (item == null) return false;

            item.Name = dto.Name;
            item.CategoryId = dto.CategoryId;
            item.UnitOfMeasureId = dto.UnitOfMeasureId;
            item.RefPrice = dto.RefPrice;
            item.MininumStock = dto.MininumStock;
            item.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.Materials.FindAsync(id);
            if (item == null) return false;
            item.IsRemoved = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}