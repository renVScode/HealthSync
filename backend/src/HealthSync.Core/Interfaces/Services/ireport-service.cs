
using HealthSync.Core.DTOs.Report;

namespace HealthSync.Core.Interfaces.Services;

public interface IReportService
{
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(DateTime from, DateTime to);
    Task<RevenueReportDto> GetRevenueAsync(DateTime from, DateTime to);
    Task<List<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime from, DateTime to);
    Task<InventorySummaryDto> GetInventorySummaryAsync();
    Task<List<PatientVisitDto>> GetPatientVisitsAsync(DateTime from, DateTime to);
}
