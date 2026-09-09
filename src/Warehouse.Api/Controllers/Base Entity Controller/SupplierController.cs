using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.Suppliers;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    private readonly ISupplierService _supplierService = supplierService;

    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers([FromQuery] PagingRequest request)
    {
        var result = await _supplierService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdSupplier(Guid id)
    {
        var result = await _supplierService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(CreateSupplierDto dto)
    {
        var result = await _supplierService.CreateAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, UpdateSupplierDto dto)
    {
        var result = await _supplierService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        var result = await _supplierService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok();
    }
}