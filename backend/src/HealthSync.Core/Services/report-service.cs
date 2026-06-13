using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs.Report;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(DateTime from, DateTime to)
    {
        var appointments = await _uow.Appointments.FindAsync(a => a.StartTime >= from && a.StartTime <= to);
        return new AppointmentSummaryDto
        {
            Total = appointments.Count(),
            Scheduled = appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
            Confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
            Completed = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            NoShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
            Period = $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}"
        };
    }

    public async Task<RevenueReportDto> GetRevenueAsync(DateTime from, DateTime to)
    {
        var billings = await _uow.Billings.FindAsync(b => b.CreatedAt >= from && b.CreatedAt <= to);
        var paid = billings.Where(b => b.Status == BillingStatus.Paid || b.Status == BillingStatus.PartiallyPaid);
        var daily = paid.GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenueDto { Date = g.Key, Revenue = g.Sum(b => b.AmountPaid), AppointmentCount = g.Count() })
            .OrderBy(d => d.Date).ToList();

        return new RevenueReportDto
        {
            TotalRevenue = paid.Sum(b => b.AmountPaid),
            TotalDiscount = billings.Sum(b => b.Discount),
            TotalTax = billings.Sum(b => b.Tax),
            InvoiceCount = billings.Count(),
            PaidCount = billings.Count(b => b.Status == BillingStatus.Paid),
            PendingCount = billings.Count(b => b.Status == BillingStatus.Pending),
            DailyBreakdown = daily
        };
    }

    public async Task<List<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime from, DateTime to)
    {
        var appointments = await _uow.Appointments.Query()
            .Include(a => a.Doctor)
            .Where(a => a.StartTime >= from && a.StartTime <= to && a.Status == AppointmentStatus.Completed)
            .ToListAsync();

        return appointments.GroupBy(a => new { a.DoctorId, a.Doctor.FirstName, a.Doctor.LastName, a.Doctor.Specialization })
            .Select(g => new DoctorPerformanceDto
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = $"{g.Key.FirstName} {g.Key.LastName}",
                Specialization = g.Key.Specialization,
                AppointmentsCompleted = g.Count(),
                PatientsSeen = g.Select(a => a.PatientId).Distinct().Count()
            }).ToList();
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync()
    {
        var batches = await _uow.InventoryBatches.Query().Include(b => b.Medicine).ToListAsync();
        return new InventorySummaryDto
        {
            TotalMedicines = batches.Select(b => b.MedicineId).Distinct().Count(),
            TotalBatches = batches.Count,
            TotalStockValue = batches.Sum(b => b.Quantity * b.UnitPrice),
            LowStockCount = batches.GroupBy(b => b.MedicineId)
                .Count(g => g.Sum(b => b.Quantity) <= g.First().Medicine.ReorderLevel),
            ExpiringCount = batches.Count(b => b.ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) && b.ExpiryDate > DateOnly.FromDateTime(DateTime.UtcNow))
        };
    }

    public async Task<List<PatientVisitDto>> GetPatientVisitsAsync(DateTime from, DateTime to)
    {
        var appointments = await _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Where(a => a.StartTime >= from && a.StartTime <= to && a.Status == AppointmentStatus.Completed)
            .ToListAsync();

        return appointments.GroupBy(a => new { a.PatientId, a.Patient.FirstName, a.Patient.LastName })
            .Select(g => new PatientVisitDto
            {
                PatientId = g.Key.PatientId,
                PatientName = $"{g.Key.FirstName} {g.Key.LastName}",
                VisitCount = g.Count(),
                LastVisit = g.Max(a => a.StartTime)
            })
            .OrderByDescending(v => v.VisitCount).ToList();
    }
}
