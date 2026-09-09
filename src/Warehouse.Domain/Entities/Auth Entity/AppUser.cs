using Microsoft.AspNetCore.Identity;


namespace Warehouse.Domain.Entities;
public class AppUser : IdentityUser<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();
    public ICollection<WarehousePermission> WarehousePermissions { get; set; } = [];
}
public class PasswordHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AppUser? User { get; set; }
}