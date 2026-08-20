using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.DTO.UnitOfMeasure;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UnitOfMeasuresController(IUnitOfMeasureService unitOfMeasureService) : ControllerBase
{
    private readonly IUnitOfMeasureService _unitOfMeasureService = unitOfMeasureService;

    [HttpGet]
    public async Task<IActionResult> GetAllUnits([FromQuery] PagingRequest request)
    {
        var result = await _unitOfMeasureService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdUnit(Guid id)
    {
        var result = await _unitOfMeasureService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPost]
    public async Task<IActionResult> CreateUnit(CreateUnitOfMeasureDto dto)
    {
        var result = await _unitOfMeasureService.CreateAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUnit(Guid id, UpdateUnitOfMeasureDto dto)
    {
        var result = await _unitOfMeasureService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUnit(Guid id)
    {
        var result = await _unitOfMeasureService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok();
    }
}