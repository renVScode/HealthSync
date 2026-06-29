using HealthSync.Core.DTOs.AuditLog;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null)
    {
        var utcFrom = from.HasValue ? (DateTime?)DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : null;
        var utcTo = to.HasValue ? (DateTime?)DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : null;
        var (items, totalCount) = await _auditService.GetLogsPaginatedAsync(page, pageSize, entityType, utcFrom, utcTo, search);

        var dtoItems = items.Select(log => new AuditLogResponseDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            CreatedAt = log.CreatedAt,
            User = log.User != null ? new AuditLogUserDto
            {
                FirstName = log.User.FirstName,
                LastName = log.User.LastName,
                Role = log.User.Role.ToString()
            } : null
        }).ToList();

        return Ok(new { items = dtoItems, totalCount });
    }
}
