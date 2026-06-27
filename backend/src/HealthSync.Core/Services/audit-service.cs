using Microsoft.EntityFrameworkCore;
using HealthSync.Core.Entities;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;

    public AuditService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task LogAsync(string action, string entityType, Guid? entityId, string? oldValues, string? newValues, string? userId, string? ipAddress, string? userAgent)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId != null ? Guid.Parse(userId) : null,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType, Guid? entityId, DateTime? from, DateTime? to)
    {
        var query = _uow.AuditLogs.Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(l => l.EntityType == entityType);
        if (entityId.HasValue) query = query.Where(l => l.EntityId == entityId);
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to);
        return await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetLogsPaginatedAsync(
        int page, int pageSize, string? entityType, DateTime? from, DateTime? to, string? search = null)
    {
        var query = _uow.AuditLogs.Query().Include(l => l.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(l => l.EntityType == entityType);
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(l => l.Action.ToLower().Contains(term)
                                  || l.EntityType.ToLower().Contains(term)
                                  || l.IpAddress!.ToLower().Contains(term)
                                  || (l.User != null && (l.User.FirstName.ToLower().Contains(term) || l.User.LastName.ToLower().Contains(term))));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
