using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace POS.API.Authorization;

/// <summary>
/// Enforces that the authenticated user holds at least one of the listed permission claims.
/// Apply via [RequirePermission(Permissions.Sales.Complete)] or multiple for OR logic.
/// </summary>
public class PermissionFilter : IAuthorizationFilter
{
    private readonly string[] _permissions;

    // permissions are passed as a semicolon-delimited string from RequirePermissionAttribute
    public PermissionFilter(string permissions)
    {
        _permissions = permissions.Split(';', StringSplitOptions.RemoveEmptyEntries);
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userPermissions = context.HttpContext.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        if (!_permissions.Any(userPermissions.Contains))
            context.Result = new ForbidResult();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    /// <param name="permissions">One or more permission strings. User must hold at least one (OR logic).</param>
    public RequirePermissionAttribute(params string[] permissions)
        : base(typeof(PermissionFilter))
    {
        Arguments = [string.Join(';', permissions)];
    }
}
