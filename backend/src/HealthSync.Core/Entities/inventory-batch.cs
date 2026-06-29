namespace HealthSync.Core.Entities;

public class InventoryBatch
{
    public Guid Id { get; set; }
    public Guid MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateOnly? ManufactureDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public string? Location { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Medicine Medicine { get; set; } = null!;
    public ICollection<InventoryTransaction> Transactions { get; set; } = [];
}
