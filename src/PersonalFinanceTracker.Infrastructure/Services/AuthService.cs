using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Auth;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

/// <summary>
/// Handles user registration and login.
/// Passwords are hashed with BCrypt; tokens are signed RS-256 JWTs.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration  _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config   = config;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        // 1. Ensure email is unique
        if (await _userRepo.ExistsAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return Result<AuthResponseDto>.Failure("Email is already registered.", 409);

        // 2. Hash password and create user
        var user = new User
        {
            FullName     = dto.FullName,
            Email        = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = "User",
            CreatedAt    = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        // 3. Issue JWT
        return Result<AuthResponseDto>.Success(BuildToken(user), 201);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password.", 401);

        return Result<AuthResponseDto>.Success(BuildToken(user));
    }

    // ── JWT construction ──────────────────────────────────────────────────────
    private AuthResponseDto BuildToken(User user)
    {
        var jwtKey   = _config["Jwt:Key"]    ?? throw new InvalidOperationException("Jwt:Key not configured.");
        var jwtIssuer = _config["Jwt:Issuer"] ?? "PersonalFinanceTracker";
        var expires  = DateTime.UtcNow.AddHours(
            int.TryParse(_config["Jwt:ExpiresHours"], out int h) ? h : 24);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer:   jwtIssuer,
            audience: jwtIssuer,
            claims:   claims,
            expires:  expires,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token    = new JwtSecurityTokenHandler().WriteToken(token),
            FullName = user.FullName,
            Email    = user.Email,
            Role     = user.Role,
            Expires  = expires
        };
    }
}
