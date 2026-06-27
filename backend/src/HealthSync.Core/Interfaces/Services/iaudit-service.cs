using HealthSync.Core.Entities;

namespace HealthSync.Core.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid? entityId, string? oldValues,
        string? newValues, string? userId,
        string? ipAddress, string? userAgent);
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType, Guid? entityId, DateTime? from, DateTime? to);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetLogsPaginatedAsync(
        int page, int pageSize, string? entityType, DateTime? from, DateTime? to, string? search = null);
}
