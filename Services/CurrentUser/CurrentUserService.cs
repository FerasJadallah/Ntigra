using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Ntigra.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
