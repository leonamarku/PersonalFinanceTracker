using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonalFinanceTracker.API.Controllers;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Auth;
using PersonalFinanceTracker.Core.Interfaces.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private AuthController CreateSut() => new(_authServiceMock.Object);

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WhenSuccessful_Returns201WithToken()
    {
        // Arrange
        var response = new AuthResponseDto { Token = "jwt.token.here", Role = "User", Email = "leona@test.com" };
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>()))
                        .ReturnsAsync(Result<AuthResponseDto>.Success(response, 201));

        var sut = CreateSut();
        var dto = new RegisterRequestDto { FullName = "Leona", Email = "leona@test.com", Password = "pass123" };

        // Act
        var actionResult = await sut.Register(dto);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
        objectResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_Returns409()
    {
        // Arrange
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>()))
                        .ReturnsAsync(Result<AuthResponseDto>.Failure("Email is already registered.", 409));

        var sut = CreateSut();
        var dto = new RegisterRequestDto { FullName = "Test", Email = "dup@test.com", Password = "pass" };

        // Act
        var actionResult = await sut.Register(dto);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(409);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WhenCredentialsValid_Returns200WithToken()
    {
        // Arrange
        var response = new AuthResponseDto { Token = "valid.jwt", Role = "User", Email = "leona@test.com" };
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                        .ReturnsAsync(Result<AuthResponseDto>.Success(response));

        var sut = CreateSut();
        var dto = new LoginRequestDto { Email = "leona@test.com", Password = "pass123" };

        // Act
        var actionResult = await sut.Login(dto);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Login_WithWrongCredentials_Returns401()
    {
        // Arrange
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                        .ReturnsAsync(Result<AuthResponseDto>.Failure("Invalid email or password.", 401));

        var sut = CreateSut();
        var dto = new LoginRequestDto { Email = "x@x.com", Password = "wrong" };

        // Act
        var actionResult = await sut.Login(dto);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(401);
    }
}
