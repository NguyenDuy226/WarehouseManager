using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Warehouse.Application.DTO.Auth;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;
using Warehouse.Infrastructure.Services;

namespace ExpenseTracker.API.Services;

public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly CreateToken _createToken;
    private readonly WarehouseDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor; 
    private readonly IConfiguration _config;
    private readonly CodeGenerator _codeGenerator;
    public AuthService(UserManager<AppUser> userManager, CreateToken createToken, WarehouseDbContext context,IHttpContextAccessor httpContextAccessor, IConfiguration config, CodeGenerator code)
    {
        _userManager = userManager;
        _createToken = createToken;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _config = config;
        _codeGenerator = code;
    }

    private async Task AddAuthAudit(Guid? userId, string action)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var ipAddress = request?.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = request?.Headers["User-Agent"].ToString();
        var log = new AuthAuditLog 
        {
            UserId = userId?.ToString(),
            Action = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuthAuditLogs.Add(log);
    }

    public async Task<(int StatusCode, string? ErrorMessage, LoginResponseDTO? Data)> LoginAsync(LoginDTO dto)
    {
        const string errorMessage = "email or pass wrong";
        
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            await AddAuthAudit(null, "LOGIN_FAILED");
            await _context.SaveChangesAsync();
            return (401, errorMessage, null);
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
            await AddAuthAudit(user.Id, "ACCOUNT_LOCKOUT");
            await _context.SaveChangesAsync();
            return (429, "ACCOUNT LOCKED", null);
        }
        if (!user.IsActive)
        {
            await _userManager.AccessFailedAsync(user);   
            await AddAuthAudit(user.Id, "ACCOUNT_NOT_ACTIVE");
            await _context.SaveChangesAsync();
            return (403, "ACCOUNT NOT ACTIVE", null);
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            await AddAuthAudit(user.Id, "LOGIN_FAILED");
            await _context.SaveChangesAsync();
            return (401, errorMessage, null);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var token = await _createToken.CreateJWT(user);
        var refreshToken = await _createToken.CreateRefreshToken(user.Id);
        await AddAuthAudit(user.Id, "LOGIN_SUCCESS");
        await _context.SaveChangesAsync();
        return (200, null, new LoginResponseDTO(token, refreshToken));
    }

    public async Task<(int StatusCode, string? ErrorMessage, AuthResponseDTO? Data)> RefreshAsync(RefreshTokenDTO dto)
    {
        try
        {
            var prin = _createToken.ReadExpiredToken(dto.Token);
            var userIdString = prin.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return (400, "Invalid token", null);

            var user = await _userManager.FindByIdAsync(userIdString);
            if (user == null || !user.IsActive)
                return (400, "Invalid user", null);

            //sercurity stamp
            var oldTokenStamp = prin.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
            if (string.IsNullOrEmpty(oldTokenStamp) || oldTokenStamp != user.SecurityStamp)
            {
                return (401, "role has changed", null);
            }

            var tempRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.RefreshTokenValue == dto.RefreshToken && t.AppUserId == userId);

            if (tempRefreshToken == null) 
                return (401, "Invalid token", null);
                
            //remove refresh token
            if (tempRefreshToken.IsRemoved)
            {
                var activeTokens = await _context.RefreshTokens.Where(t => t.AppUserId == userId).ToListAsync();
                _context.RefreshTokens.RemoveRange(activeTokens);

                await AddAuthAudit(userId, "TOKEN_REUSE_DETECTED");
                await _context.SaveChangesAsync();

                return (401, "plis try again", null);
            }

            if (tempRefreshToken.IsExpired) 
                return (401, "token expired", null);

            tempRefreshToken.IsRemoved = true; 
            var newToken = await _createToken.CreateJWT(user);
            var newRefreshToken = await _createToken.CreateRefreshToken(userId);
            
            await AddAuthAudit(userId, "TOKEN_REFRESHED");
            await _context.SaveChangesAsync();
            
            return (200, null, new AuthResponseDTO(newToken, newRefreshToken));
        }
        catch (Exception ex)
        {
            return (400, $"Refresh token failed: {ex.Message}", null);
        }
    }

    public async Task<(int StatusCode, string? ErrorMessage)> LogoutAsync()
    {
        var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)?? null;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return (401, "invalid user");
        }
        var allTokens = await _context.RefreshTokens
            .Where(t => t.AppUserId == userId)
            .ToListAsync();

        if (allTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(allTokens);
        }

        await AddAuthAudit(userId, "LOGOUT");
        await _context.SaveChangesAsync();

        return (200, null);
    }

    public async Task<(int StatusCode, string? ErrorMessage)> ChangePasswordAsync(Guid userId, ChangePasswordDTO dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return (404, "user not found");

        var checkOldPass = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
        if (!checkOldPass) 
        {
            await AddAuthAudit(userId, "CHANGE_PASSWORD_FAILED");
            await _context.SaveChangesAsync();
            return (400, "wrong pass");
        }
        var passwordHistoryCount = _config.GetValue<int>("SecuritySettings:PasswordHistoryCount");
        var oldHistories = await _context.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(passwordHistoryCount)
            .ToListAsync();

        var passwordHasher = _userManager.PasswordHasher;
        foreach (var history in oldHistories)
        {
            var verifyResult = passwordHasher.VerifyHashedPassword(user, history.PasswordHash, dto.NewPassword);
            if (verifyResult != PasswordVerificationResult.Failed)
            {
                return (400, $"Mật khẩu không được trùng với {passwordHistoryCount} mật khẩu gần nhất.");
            }
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (400, $"Đổi mật khẩu thất bại: {errors}");
        }
        _context.PasswordHistories.Add(new PasswordHistory 
        { 
            UserId = userId, 
            PasswordHash = user.PasswordHash!
        });

        var activeTokens = await _context.RefreshTokens.Where(t => t.AppUserId == userId).ToListAsync();
        foreach (var t in activeTokens) t.IsRemoved = true;

        await AddAuthAudit(userId, "PASSWORD_CHANGED");
        await _context.SaveChangesAsync();

        return (200, null);
    }

    public async Task<(int StatusCode, string? ErrorMessage, object? Errors)> RegisterAsync(RegisterDTO dto)
    {
        var user = new AppUser()
        {
            Name = dto.Name,
            Email = dto.Email,
            UserName = dto.Email,
            IsActive = true,
            Code = await _codeGenerator.GenerateCode<AppUser>(),

        };

        var checkEmail = await _userManager.FindByEmailAsync(user.Email);
        if (checkEmail != null) return (409, "existed email", null);

        var create = await _userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded) return (400, "create fail", create.Errors);

        var addRole = await _userManager.AddToRoleAsync(user, "USER");
        if (!addRole.Succeeded) return (400, "add role failed", addRole.Errors);

        await AddAuthAudit(user.Id, "REGISTER_SUCCESS");
        await _context.SaveChangesAsync();

        return (200, null, null);
    }

}