using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>
/// Base class for all resource controllers.
/// Provides helper methods to extract the authenticated user's ID and role from the JWT.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Returns the authenticated user's ID from the JWT NameIdentifier claim.</summary>
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found."));

    /// <summary>Returns true if the authenticated user has the Admin role.</summary>
    protected bool IsAdmin =>
        User.IsInRole("Admin");
}
