namespace HealthSync.Core.Entities;

public class Medicine
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // brand name
    public string? GenericName { get; set; } // generic / active ingredient
    public string? Category { get; set; } // e.g. antibiotic, analgesic, etc.
    public string? Manufacturer { get; set; }
    public string Unit { get; set; } = string.Empty; // tablet, vial, capsult. etc.``
    public decimal Price { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public ICollection<InventoryBatch> InventoryBatches { get; set; } = [];
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
