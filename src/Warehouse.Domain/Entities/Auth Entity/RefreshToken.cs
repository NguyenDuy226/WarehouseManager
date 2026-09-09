namespace Warehouse.Domain.Entities;
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppUserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string RefreshTokenValue { get; set; } = null!;
    public DateTime Expire { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expire;
    public DateTime Create { get; set; } = DateTime.UtcNow;
    public bool IsRemoved { get; set; } = false;

}