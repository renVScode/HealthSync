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
        return Ok(new { items, totalCount });
    }
}
