using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.Warehouse;
using Warehouse.Application.Interfaces;
using Warehouse.Infrastructure.Services.Warehouse;

namespace Warehouse.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    private readonly IWarehouseService _warehouseService = warehouseService;
    [HttpGet]
    public async Task<IActionResult> GetAllWarehouse([FromQuery]PagingRequest request)
    {
        var result = await _warehouseService.GetAllAsync(request);
        return Ok(result);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdWarehouse(Guid id)
    {
        var result = await _warehouseService.GetByIdAsync(id);
        return Ok(result);
    }
    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPost]
    public async Task<IActionResult> CreateWarehouse(CreateWarehouseDto dto)
    {
        var result = await _warehouseService.CreateAsync(dto);
        return Ok(result);
    }
    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, UpdateWarehouseDto dto)
    {
        var result = await _warehouseService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }
    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWarehouse(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if(!result) return NotFound();
        return Ok();     
    }
    

}