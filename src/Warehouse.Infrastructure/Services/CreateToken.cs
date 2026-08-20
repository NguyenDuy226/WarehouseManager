using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace ExpenseTracker.API.Services
{
    public class CreateToken
    {
        public readonly UserManager<AppUser> _userManager;
        public readonly IConfiguration _iconfiguration;
        public readonly WarehouseDbContext _context;
        public CreateToken (UserManager<AppUser> userManager, IConfiguration _configuration, WarehouseDbContext context)
        {
            _userManager = userManager;
            _iconfiguration = _configuration;
            _context = context;
        }
        public async Task<string> CreateJWT(AppUser user)
        {
            //key, cred, claim
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iconfiguration["JWT:Key"]?? string.Empty));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var expires = _iconfiguration.GetValue<int>("JWT:ExpiryMinute");
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: cred,
                expires: DateTime.UtcNow.AddMinutes(expires),
                issuer: _iconfiguration["JWT:Issuer"],
                audience: _iconfiguration["JWT:Audience"]
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshToken(Guid userId)
        {
            var refreshTokenString = Guid.NewGuid();
            var days = _iconfiguration.GetValue<int>("Jwt:RefreshTokenExpiryDay", 7);

            var refreshToken = new RefreshToken
            {
                RefreshTokenValue = refreshTokenString.ToString(),
                AppUserId = userId,
                Expire = DateTime.UtcNow.AddDays(days),
                IsRemoved = false
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshTokenString.ToString();
        }
        public ClaimsPrincipal ReadExpiredToken(string token)
        {
            var secretKey = _iconfiguration["Jwt:Key"] ?? string.Empty;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false 
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var prin = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("invalid token");
            }
            return prin;
        }
        
    }
}