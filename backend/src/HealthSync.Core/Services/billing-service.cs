using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs.Billing;
using HealthSync.Core.Entities;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class BillingService : IBillingService
{
    private readonly IUnitOfWork _uow;

    public BillingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PaginatedResult<BillingResponseDto>> GetAllAsync(BillingFilterDto filter)
    {
        var query = _uow.Billings.Query()
            .Include(b => b.Patient)
            .Include(b => b.Items)
            .Include(b => b.Payments)
            .AsQueryable();

        if (filter.PatientId.HasValue) query = query.Where(b => b.PatientId == filter.PatientId);
        if (filter.Status.HasValue) query = query.Where(b => b.Status == filter.Status);
        if (filter.DateFrom.HasValue) query = query.Where(b => b.CreatedAt >= filter.DateFrom);
        if (filter.DateTo.HasValue) query = query.Where(b => b.CreatedAt <= filter.DateTo);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.CreatedAt)
                               .Skip((filter.Page - 1) * filter.PageSize)
                               .Take(filter.PageSize)
                               .ToListAsync();

        return new PaginatedResult<BillingResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<BillingResponseDto?> GetByIdAsync(Guid id)
    {
        var billing = await _uow.Billings.Query()
            .Include(b => b.Patient)
            .Include(b => b.Items)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);

        return billing == null ? null : MapToDto(billing);
    }

    public async Task<BillingResponseDto> CreateAsync(CreateBillingDto dto)
    {
        var subTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
        var total = subTotal - dto.Discount + dto.Tax;

        var billing = new Billing
        {
            PatientId = dto.PatientId,
            AppointmentId = dto.AppointmentId,
            InvoiceNumber = await GenerateInvoiceNumberAsync(),
            SubTotal = subTotal,
            Discount = dto.Discount,
            Tax = dto.Tax,
            Total = total,
            DueDate = dto.DueDate,
            Notes = dto.Notes,
            Items = dto.Items.Select(i => new BillingItem
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Quantity * i.UnitPrice
            }).ToList()
        };

        await _uow.Billings.AddAsync(billing);
        await _uow.SaveChangesAsync();
        return MapToDto(billing);
    }

    public async Task<bool> AddPaymentAsync(Guid billingId, CreatePaymentDto dto)
    {
        var billing = await _uow.Billings.Query()
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == billingId);

        if (billing == null || billing.Status == BillingStatus.Paid || billing.Status == BillingStatus.Cancelled)
            return false;

        var payment = new Payment
        {
            BillingId = billingId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            TransactionReference = dto.TransactionReference,
            Notes = dto.Notes
        };

        await _uow.Payments.AddAsync(payment);
        billing.AmountPaid += dto.Amount;
        billing.Status = billing.AmountPaid >= billing.Total ? BillingStatus.Paid : BillingStatus.PartiallyPaid;
        billing.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<PaymentResponseDto>> GetPaymentsAsync(Guid billingId)
    {
        var payments = await _uow.Payments.FindAsync(p => p.BillingId == billingId);
        return payments.Select(p => new PaymentResponseDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            ReceivedAt = p.ReceivedAt
        }).ToList();
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _uow.Billings.Query().CountAsync(b => b.CreatedAt.Year == year);
        return $"INV-{year}-{(count + 1):D4}";
    }

    private static BillingResponseDto MapToDto(Billing b) => new()
    {
        Id = b.Id,
        PatientId = b.PatientId,
        PatientName = $"{b.Patient.FirstName} {b.Patient.LastName}",
        AppointmentId = b.AppointmentId,
        InvoiceNumber = b.InvoiceNumber,
        SubTotal = b.SubTotal,
        Discount = b.Discount,
        Tax = b.Tax,
        Total = b.Total,
        AmountPaid = b.AmountPaid,
        Status = b.Status,
        CreatedAt = b.CreatedAt,
        Items = b.Items.Select(i => new BillingItemResponseDto
        {
            Id = i.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Total = i.Total
        }).ToList(),
        Payments = b.Payments.Select(p => new PaymentResponseDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            ReceivedAt = p.ReceivedAt
        }).ToList()
    };
}
