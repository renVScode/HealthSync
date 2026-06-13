namespace HealthSync.Core.DTOs.Report;

public class AppointmentSummaryDto
{
    public int Total { get; set; }
    public int Scheduled { get; set; }
    public int Confirmed { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public int InvoiceCount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = [];
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}

public class DoctorPerformanceDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int AppointmentsCompleted { get; set; }
    public int PatientsSeen { get; set; }
    public decimal RevenueGenerated { get; set; }
}

public class InventorySummaryDto
{
    public int TotalMedicines { get; set; }
    public int TotalBatches { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringCount { get; set; }
}

public class PatientVisitDto
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public DateTime LastVisit { get; set; }
    public string MostCommonDiagnosis { get; set; } = string.Empty;
}
