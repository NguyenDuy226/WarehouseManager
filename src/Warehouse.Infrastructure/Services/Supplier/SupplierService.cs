using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.Suppliers;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;
using System.Text.RegularExpressions; 


namespace Warehouse.Infrastructure.Services.Suppliers
{
    public class SupplierService : ISupplierService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;
        private readonly IValidatorName _validator;

        public SupplierService(WarehouseDbContext context, CodeGenerator codeGenerator, IValidatorName validator)
        {
            _validator = validator;
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
            string validName = _validator.ValidHumanName(dto.Name);

            if (string.IsNullOrEmpty(validName))
                throw new Exception("Supplier name cant be null");

            var isExist = await _context.Suppliers.AnyAsync(s => s.Name.ToLower() == validName.ToLower() && !s.IsRemoved);
                
            if (isExist) throw new Exception($"Supplier '{validName}' already exist");

            string contact = dto.Contact?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(contact))
            {
                if (contact.Contains("@"))
                {
                    if (!Regex.IsMatch(contact, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        throw new Exception("invalid email");
                }
                else
                {
                    if (contact.Any(char.IsLetter))
                        throw new Exception("invalid phone number");
                }
            }

            var item = new Supplier
            {
                Id = Guid.NewGuid(),
                Code = await _codeGenerator.GenerateCode<Supplier>(),
                Name = validName, 
                TaxCode = dto.TaxCode.Trim(),
                Addres = dto.Addres.Trim(),
                Contact = contact,
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

            string validName = _validator.ValidHumanName(dto.Name);
            if (string.IsNullOrEmpty(validName)) throw new Exception("supplier name cant be null");

            if (item.Name.ToLower() != validName.ToLower())
            {
                var isExist = await _context.Suppliers
                    .AnyAsync(s => s.Id != id && s.Name.ToLower() == validName.ToLower() && !s.IsRemoved);
                    
                if (isExist)
                    throw new Exception($"supplier '{validName}' already exist");
            }

            string contact = dto.Contact?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(contact))
            {
                if (contact.Contains("@"))
                {
                    if (!Regex.IsMatch(contact, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        throw new Exception("invalid email");
                }
                else
                {
                    if (contact.Any(char.IsLetter))
                        throw new Exception("invalid phone number");
                }
            }

            item.Name = validName;
            item.TaxCode = dto.TaxCode.Trim();
            item.Addres = dto.Addres.Trim();
            item.Contact = contact;
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