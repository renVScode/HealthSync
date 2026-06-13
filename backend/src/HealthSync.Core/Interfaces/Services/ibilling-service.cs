using HealthSync.Core.DTOs.Billing;

namespace HealthSync.Core.Interfaces.Services;

public interface IBillingService
{
    Task<PaginatedResult<BillingResponseDto>> GetAllAsync(BillingFilterDto filter);
    Task<BillingResponseDto?> GetByIdAsync(Guid id);
    Task<BillingResponseDto> CreateAsync(CreateBillingDto dto);
    Task<bool> AddPaymentAsync(Guid billingId, CreatePaymentDto dto);
    Task<List<PaymentResponseDto>> GetPaymentsAsync(Guid billingId);
}
