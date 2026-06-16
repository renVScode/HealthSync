using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthSync.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RoleAuthorizationFilter : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleAuthorizationFilter(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userRole = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
