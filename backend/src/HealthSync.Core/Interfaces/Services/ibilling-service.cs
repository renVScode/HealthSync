using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Billing;

namespace HealthSync.Core.Interfaces.Services;

public interface IBillingService
{
    Task<PaginatedResult<BillingResponseDto>> GetAllAsync(BillingFilterDto filter);
    Task<BillingResponseDto?> GetByIdAsync(Guid id);
    Task<BillingResponseDto?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<BillingResponseDto> CreateAsync(CreateBillingDto dto, Guid createdById);
    Task<bool> AddPaymentAsync(Guid billingId, CreatePaymentDto dto);
    Task<List<PaymentResponseDto>> GetPaymentsAsync(Guid billingId);
    Task<bool> VerifyPaymentAsync(Guid paymentId, string userId);
    Task<bool> UploadPaymentQrCodeAsync(Guid paymentId, string imageUrl);
    Task<BillingResponseDto?> GenerateInvoiceFromVisitAsync(Guid appointmentId, string userId);
}
