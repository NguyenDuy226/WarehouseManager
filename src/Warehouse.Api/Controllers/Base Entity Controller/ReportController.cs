using System.Security.Claims;
using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Report.DTO;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly CreateToken _createToken;
    private readonly WarehouseDbContext _context;
    public ReportController (UserManager<AppUser> userManager,  RoleManager<AppRole> roleManager, CreateToken createJWT, WarehouseDbContext context)
    {   
        _userManager = userManager;
        _roleManager = roleManager;
        _createToken =  createJWT;
        _context = context;
    }

    ///GET api/report/profile
    //[Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Message = "invalid user" });
        }
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if(user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var profile = new ProfileDTO(
            user.Id,
            user.Name,
            user.Email ?? string.Empty,
            user.IsActive,
            roles
        );
        return Ok(profile);
    }
    

}