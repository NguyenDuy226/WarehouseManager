using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Categories;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.Categories
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;

        public MaterialCategoryService(WarehouseDbContext context, CodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<PagedResult<MaterialCategoryDto>> GetAllAsync(PagingRequest request)
        {
            var query = _context.MaterialCategories.AsQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new MaterialCategoryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Description = c.Description,
                    Status = c.Status
                })
                .ToListAsync();

            return new PagedResult<MaterialCategoryDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<MaterialCategoryDto?> GetByIdAsync(Guid id)
        {
            var c = await _context.MaterialCategories.FindAsync(id);
            if (c == null) return null;

            return new MaterialCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status
            };
        }

        public async Task<MaterialCategoryDto> CreateAsync(CreateMaterialCategoryDto dto)
        {
            var item = new MaterialCategory
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<MaterialCategory>(),
                Name = dto.Name,
                Description = dto.Description,
                Status = EntityStatus.Active
            };

            await _context.MaterialCategories.AddAsync(item);
            await _context.SaveChangesAsync();

            return new MaterialCategoryDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                Description = item.Description,
                Status = item.Status
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateMaterialCategoryDto dto)
        {
            var item = await _context.MaterialCategories.FindAsync(id);
            if (item == null) return false;

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.MaterialCategories.FindAsync(id);
            if (item == null) return false;
            item.IsRemoved = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}