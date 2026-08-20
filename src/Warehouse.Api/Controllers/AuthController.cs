using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.Auth;
using ExpenseTracker.API.Services; 

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var (statusCode, errorMessage, data) = await _authService.LoginAsync(dto);
        if (statusCode != 200) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(data);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDTO dto)
    {
        var (statusCode, errorMessage, data) = await _authService.RefreshAsync(dto);
        if (statusCode != 200) return StatusCode(statusCode, new { message = errorMessage });
        return Ok(data);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(new { Status = 401, Title = "Unauthorized", Error = new[] { "user not found" } });
        }
        var (statusCode, errorMessage) = await _authService.LogoutAsync();
        if (statusCode != 200)
        {
            return StatusCode(statusCode, new { Status = statusCode, Title = "Logout failed", Error = new[] { errorMessage } });
        }

        return Ok(new { Status = 200, Message = "logout" });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var (statusCode, errorMessage) = await _authService.ChangePasswordAsync(userId, dto);    
        if (statusCode != 200) return StatusCode(statusCode, new { message = errorMessage });
        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        var (statusCode, errorMessage, errors) = await _authService.RegisterAsync(dto);       
        if (statusCode != 200)
        {
            return errors != null ? BadRequest(errors) : StatusCode(statusCode, new { message = errorMessage });
        }
        return Ok();
    }

    [Authorize]
    [HttpGet("check")]
    public IActionResult Checked() => Ok(new { message = "checked" });

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }
}