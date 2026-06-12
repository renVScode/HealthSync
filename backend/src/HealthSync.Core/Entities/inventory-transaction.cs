using HealthSync.Core.Enums;

namespace HealthSync.Core.Entities;

public class InventoryTransaction
{
    public Guid Id { get; set; }
    public Guid InventoryBatchId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public InventoryBatch InventoryBatch { get; set; } = null!;
    public Identity.ApplicationUser CreatedBy { get; set; } = null!;
}
