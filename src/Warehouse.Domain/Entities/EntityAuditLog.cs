namespace Warehouse.Domain.Entities
{
    public class EntityAuditLog
    {
        public Guid Id { get; set; }        
        public Guid? UserId { get; set; } 
        public string ActionType { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string? OldValues { get; set; } 
        public string? NewValues { get; set; } 
        public string? IpAddress { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}