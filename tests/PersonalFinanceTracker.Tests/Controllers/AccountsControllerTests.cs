using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonalFinanceTracker.API.Controllers;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Account;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _serviceMock = new();

    /// <summary>
    /// Builds the controller and injects a fake ClaimsPrincipal
    /// so CurrentUserId returns 10 without a real JWT.
    /// </summary>
    private AccountsController CreateSut()
    {
        var controller = new AccountsController(_serviceMock.Object);
        var claims     = new[] { new Claim(ClaimTypes.NameIdentifier, "10") };
        var identity   = new ClaimsIdentity(claims, "Test");
        var principal  = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithAccounts()
    {
        // Arrange
        var accounts = new List<AccountDto>
        {
            new() { Id = 1, Name = "Main", Type = AccountType.Checking, Balance = 1000, Currency = "USD" }
        };
        _serviceMock.Setup(s => s.GetUserAccountsAsync(10))
                    .ReturnsAsync(Result<IEnumerable<AccountDto>>.Success(accounts));

        var sut = CreateSut();

        // Act
        var result = await sut.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(accounts);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        // Arrange
        var account = new AccountDto { Id = 5, Name = "Savings", Type = AccountType.Savings, Balance = 5000, Currency = "USD" };
        _serviceMock.Setup(s => s.GetByIdAsync(5, 10))
                    .ReturnsAsync(Result<AccountDto>.Success(account));

        // Act
        var result = await CreateSut().GetById(5);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(99, 10))
                    .ReturnsAsync(Result<AccountDto>.Failure("Account not found.", 404));

        // Act
        var result = await CreateSut().GetById(99);

        // Assert
        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_ValidDto_Returns201()
    {
        // Arrange
        var created = new AccountDto { Id = 7, Name = "New", Type = AccountType.Cash, Balance = 500, Currency = "EUR" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateAccountDto>(), 10))
                    .ReturnsAsync(Result<AccountDto>.Success(created, 201));

        var dto = new CreateAccountDto { Name = "New", Type = AccountType.Cash, Balance = 500, Currency = "EUR" };

        // Act
        var result = await CreateSut().Create(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Delete_WhenOwner_Returns204()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(3, 10))
                    .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await CreateSut().Delete(3);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
