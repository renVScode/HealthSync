namespace HealthSync.Core.Entities;

public class BillingItem
{
    public Guid Id { get; set; }
    public Guid BillingId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    // navigation
    public Billing Billing { get; set; } = null!;
}
