
using HealthSync.Core.DTOs.Report;

namespace HealthSync.Core.Interfaces.Services;

public interface IReportService
{
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(DateTime from, DateTime to, Guid? doctorId = null);
    Task<RevenueReportDto> GetRevenueAsync(DateTime from, DateTime to, Guid? doctorId = null);
    Task<List<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime from, DateTime to);
    Task<InventorySummaryDto> GetInventorySummaryAsync();
    Task<List<PatientVisitDto>> GetPatientVisitsAsync(DateTime from, DateTime to, Guid? doctorId = null);
}
