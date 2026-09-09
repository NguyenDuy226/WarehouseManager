using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Categories;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MaterialCategoriesController(IMaterialCategoryService materialCategoryService) : ControllerBase
{
    private readonly IMaterialCategoryService _materialCategoryService = materialCategoryService;

    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] PagingRequest request)
    {
        var result = await _materialCategoryService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdCategory(Guid id)
    {
        var result = await _materialCategoryService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateMaterialCategoryDto dto)
    {
        var result = await _materialCategoryService.CreateAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateMaterialCategoryDto dto)
    {
        var result = await _materialCategoryService.UpdateAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "SYSTEM_ADMIN,WAREHOUSE_MANAGER")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _materialCategoryService.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok();
    }
}