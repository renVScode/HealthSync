using HealthSync.Core.Enums;

namespace HealthSync.Core.Entities;

public class Billing
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public BillingStatus Status { get; set; } = BillingStatus.Pending;
    public DateOnly? DueDate { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Patient Patient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public Identity.ApplicationUser CreatedBy { get; set; } = null!;
    public ICollection<BillingItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
