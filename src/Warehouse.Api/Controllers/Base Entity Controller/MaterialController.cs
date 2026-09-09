using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Materials;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers;

[Authorize (Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, APPROVER, REQUESTER, AUDITOR")]
[ApiController]
[Route("api/[controller]")]
public class MaterialsController(IMaterialService materialService) : ControllerBase
{
    private readonly IMaterialService _materialService = materialService;

    [HttpGet]
    public async Task<IActionResult> GetAllMaterials([FromQuery] PagingRequest request)
    {
        var result = await _materialService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdMaterial(Guid id)
    {
        var result = await _materialService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMaterial(CreateMaterialDto dto)
    {
        var result = await _materialService.CreateAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, UpdateMaterialDto dto)
    {
        var result = await _materialService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMaterial(Guid id)
    {
        var result = await _materialService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok();
    }
}