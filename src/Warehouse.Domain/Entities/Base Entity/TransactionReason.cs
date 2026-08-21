using Warehouse.Domain.Inventory.Enums;

namespace Warehouse.Domain.Entities
{
    public class TransactionReason
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public EntityStatus Status { get; set; }
        public bool IsRemoved { get; set; } = false;
    }
}