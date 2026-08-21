using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Auth;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>Handles user registration and login. No authentication required.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(result.StatusCode, result.Data);
    }

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }
}
