using Microsoft.Extensions.Configuration;
using Moq;
using PersonalFinanceTracker.Core.DTOs.Auth;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly IConfiguration _config;

    public AuthServiceTests()
    {
        // Build an in-memory configuration with a valid JWT key
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]          = "test-secret-key-that-is-at-least-32-chars!",
                ["Jwt:Issuer"]       = "TestIssuer",
                ["Jwt:ExpiresHours"] = "1"
            })
            .Build();
    }

    private AuthService CreateSut() => new(_userRepoMock.Object, _config);

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithNewEmail_ReturnsSuccessWithToken()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                     .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var sut = CreateSut();
        var dto = new RegisterRequestDto { FullName = "Leona Marku", Email = "leona@test.com", Password = "secret123" };

        // Act
        var result = await sut.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.Role.Should().Be("User");
        result.Data.Email.Should().Be("leona@test.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                     .ReturnsAsync(true);

        var sut = CreateSut();
        var dto = new RegisterRequestDto { FullName = "Leona", Email = "exists@test.com", Password = "pass" };

        // Act
        var result = await sut.RegisterAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Error.Should().Contain("already registered");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserInfo()
    {
        // Arrange — store a BCrypt hash so the service can verify it
        var hash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var user = new User { Id = 1, FullName = "Leona", Email = "leona@test.com", PasswordHash = hash, Role = "User" };

        _userRepoMock.Setup(r => r.GetByEmailAsync("leona@test.com")).ReturnsAsync(user);

        var sut = CreateSut();
        var dto = new LoginRequestDto { Email = "leona@test.com", Password = "correct-password" };

        // Act
        var result = await sut.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.FullName.Should().Be("Leona");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var user = new User { Id = 1, Email = "leona@test.com", PasswordHash = hash, Role = "User" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("leona@test.com")).ReturnsAsync(user);

        var sut = CreateSut();
        var dto = new LoginRequestDto { Email = "leona@test.com", Password = "wrong-password" };

        // Act
        var result = await sut.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var sut = CreateSut();
        var dto = new LoginRequestDto { Email = "nobody@test.com", Password = "any" };

        // Act
        var result = await sut.LoginAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }
}
