using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.Suppliers;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.Suppliers
{
    public class SupplierService : ISupplierService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;

        public SupplierService(WarehouseDbContext context, CodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        public async Task<PagedResult<SupplierDto>> GetAllAsync(PagingRequest request)
        {
            var query = _context.Suppliers.AsQueryable();
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    TaxCode = s.TaxCode,
                    Addres = s.Addres,
                    Contact = s.Contact,
                    Status = s.Status
                })
                .ToListAsync();

            return new PagedResult<SupplierDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<SupplierDto?> GetByIdAsync(Guid id)
        {
            var s = await _context.Suppliers.FindAsync(id);
            if (s == null) return null;

            return new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                TaxCode = s.TaxCode,
                Addres = s.Addres,
                Contact = s.Contact,
                Status = s.Status
            };
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var item = new Supplier
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<Supplier>(),
                Name = dto.Name,
                TaxCode = dto.TaxCode,
                Addres = dto.Addres,
                Contact = dto.Contact,
                Status = EntityStatus.Active
            };

            await _context.Suppliers.AddAsync(item);
            await _context.SaveChangesAsync();

            return new SupplierDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                TaxCode = item.TaxCode,
                Addres = item.Addres,
                Contact = item.Contact,
                Status = item.Status
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto)
        {
            var item = await _context.Suppliers.FindAsync(id);
            if (item == null) return false;

            item.Name = dto.Name;
            item.TaxCode = dto.TaxCode;
            item.Addres = dto.Addres;
            item.Contact = dto.Contact;
            item.Status = dto.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.Suppliers.FindAsync(id);
            if (item == null) return false;
            item.IsRemoved = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}