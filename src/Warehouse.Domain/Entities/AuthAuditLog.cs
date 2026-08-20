namespace Warehouse.Domain.Entities;

public class AuthAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? UserId { get; set; } 
    public string Action { get; set; } = string.Empty; 
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}