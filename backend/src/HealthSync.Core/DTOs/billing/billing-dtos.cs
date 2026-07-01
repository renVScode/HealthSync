using HealthSync.Core.Enums;

namespace HealthSync.Core.DTOs.Billing;

public class CreateBillingDto
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateBillingItemDto> Items { get; set; } = [];
}

public class CreateBillingItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? PaymentDetails { get; set; }
    public string? Notes { get; set; }
}

public class BillingResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance => Total - AmountPaid;
    public BillingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BillingItemResponseDto> Items { get; set; } = [];
    public List<PaymentResponseDto> Payments { get; set; } = [];
}

public class BillingItemResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public string? PaymentDetails { get; set; }
    public bool IsVerified { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public class BillingFilterDto
{
    public Guid? PatientId { get; set; }
    public BillingStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

