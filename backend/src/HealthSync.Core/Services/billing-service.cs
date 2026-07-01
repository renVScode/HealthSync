using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Billing;
using HealthSync.Core.Entities;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class BillingService : IBillingService
{
    private readonly IUnitOfWork _uow;
    private readonly IMedicalRecordService _medicalRecordService;

    public BillingService(IUnitOfWork uow, IMedicalRecordService medicalRecordService)
    {
        _uow = uow;
        _medicalRecordService = medicalRecordService;
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
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLower();
            query = query.Where(b => b.InvoiceNumber.ToLower().Contains(term)
                                  || b.Patient.FirstName.ToLower().Contains(term)
                                  || b.Patient.LastName.ToLower().Contains(term));
        }

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

    public async Task<BillingResponseDto?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        var billing = await _uow.Billings.Query()
            .Include(b => b.Patient)
            .Include(b => b.Items)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.AppointmentId == appointmentId);

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

    public async Task<BillingResponseDto?> GenerateInvoiceFromVisitAsync(Guid appointmentId, string userId)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.ServiceOffering)
            .Include(a => a.MedicalRecord).ThenInclude(r => r.Prescriptions).ThenInclude(p => p.Medicine)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null || appointment.MedicalRecord == null)
            return null;

        var medicalRecord = appointment.MedicalRecord;

        var items = new List<CreateBillingItemDto>();

        if (appointment.ServiceOffering != null)
        {
            items.Add(new()
            {
                Description = $"{appointment.ServiceOffering.ServiceName} - Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                Quantity = 1,
                UnitPrice = appointment.ServiceOffering.Price
            });
        }
        else
        {
            items.Add(new()
            {
                Description = $"Consultation - Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                Quantity = 1,
                UnitPrice = appointment.Doctor.ConsultationFee
            });
        }

        foreach (var prescription in medicalRecord.Prescriptions)
        {
            items.Add(new CreateBillingItemDto
            {
                Description = $"{prescription.Medicine.Name} ({prescription.Dosage}, {prescription.Frequency})",
                Quantity = prescription.Quantity,
                UnitPrice = prescription.Medicine.Price
            });
        }

        var createDto = new CreateBillingDto
        {
            PatientId = appointment.PatientId,
            AppointmentId = appointmentId,
            Items = items
        };

        var billing = await CreateAsync(createDto);

        if (string.IsNullOrEmpty(appointment.Token))
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var todayCount = await _uow.Appointments.Query()
                .CountAsync(a => a.CreatedAt.Date == DateTime.UtcNow.Date);
            appointment.Token = $"T-{datePart}-{todayCount + 1:D4}";
            await _uow.SaveChangesAsync();
        }

        return billing;
    }

    public async Task<bool> AddPaymentAsync(Guid billingId, CreatePaymentDto dto)
    {
        var billing = await _uow.Billings.Query()
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == billingId);

        if (billing == null || billing.Status == BillingStatus.Paid || billing.Status == BillingStatus.Cancelled)
            return false;

        var hasQr = !string.IsNullOrWhiteSpace(dto.QrCodeImageUrl);
        var payment = new Payment
        {
            BillingId = billingId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            TransactionReference = dto.TransactionReference,
            QrCodeImageUrl = dto.QrCodeImageUrl,
            PaymentDetails = dto.PaymentDetails,
            IsVerified = !hasQr,
            Notes = dto.Notes
        };

        await _uow.Payments.AddAsync(payment);
        if (payment.IsVerified)
        {
            billing.AmountPaid += dto.Amount;
            billing.Status = billing.AmountPaid >= billing.Total ? BillingStatus.Paid : BillingStatus.PartiallyPaid;
        }

        if (billing.Status == BillingStatus.Paid && billing.AppointmentId.HasValue)
        {
            var appointment = await _uow.Appointments.Query()
                .Include(a => a.MedicalRecord)
                .FirstOrDefaultAsync(a => a.Id == billing.AppointmentId);

            if (appointment?.MedicalRecord != null)
                await _medicalRecordService.MarkPrescriptionsAsPaidAsync(appointment.MedicalRecord.Id);
        }

        billing.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyPaymentAsync(Guid paymentId, string userId)
    {
        var payment = await _uow.Payments.Query()
            .Include(p => p.Billing)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null || payment.IsVerified || payment.Billing.Status == BillingStatus.Cancelled)
            return false;

        payment.IsVerified = true;
        payment.Billing.AmountPaid += payment.Amount;
        payment.Billing.Status = payment.Billing.AmountPaid >= payment.Billing.Total
            ? BillingStatus.Paid : BillingStatus.PartiallyPaid;
        payment.Billing.UpdatedAt = DateTime.UtcNow;

        if (payment.Billing.Status == BillingStatus.Paid && payment.Billing.AppointmentId.HasValue)
        {
            var appointment = await _uow.Appointments.Query()
                .Include(a => a.MedicalRecord)
                .FirstOrDefaultAsync(a => a.Id == payment.Billing.AppointmentId);

            if (appointment?.MedicalRecord != null)
                await _medicalRecordService.MarkPrescriptionsAsPaidAsync(appointment.MedicalRecord.Id);
        }

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UploadPaymentQrCodeAsync(Guid paymentId, string imageUrl)
    {
        var payment = await _uow.Payments.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.QrCodeImageUrl = imageUrl;
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
            QrCodeImageUrl = p.QrCodeImageUrl,
            PaymentDetails = p.PaymentDetails,
            IsVerified = p.IsVerified,
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
            QrCodeImageUrl = p.QrCodeImageUrl,
            PaymentDetails = p.PaymentDetails,
            IsVerified = p.IsVerified,
            ReceivedAt = p.ReceivedAt
        }).ToList()
    };
}
