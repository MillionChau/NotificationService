using System.Security.Claims;
using Notification.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Notification.Infrastructure.Services;

/// <summary>
/// Đọc user hiện tại từ JWT claims. Contract khớp IdentityService:
/// userId ở claim "sub"/"user_id", role ở ClaimTypes.Role + "roles".
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var http = _httpContextAccessor.HttpContext;
            if (http?.User?.Identity?.IsAuthenticated != true) return null;

            var claim = http.User.FindFirst(ClaimTypes.NameIdentifier)   // "sub" sau map của JwtSecurityTokenHandler
                        ?? http.User.FindFirst("sub")
                        ?? http.User.FindFirst("user_id");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? Role =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("roles")?.Value;
}
