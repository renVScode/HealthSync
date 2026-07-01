namespace HealthSync.Core.Entities;

public class LabTest
{
    public Guid Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public ICollection<LabOrder> LabOrders { get; set; } = [];
}
