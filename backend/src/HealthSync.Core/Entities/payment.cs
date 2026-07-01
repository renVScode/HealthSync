using HealthSync.Core.Enums;

namespace HealthSync.Core.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BillingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? PaymentDetails { get; set; }
    public bool IsVerified { get; set; }
    public Guid ReceivedById { get; set; }
    public string? Notes { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Billing Billing { get; set; } = null!;
    public Identity.ApplicationUser ReceivedBy { get; set; } = null!;
}
