using AutoMapper;
using Moq;
using PersonalFinanceTracker.Core.DTOs.Account;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Mappings;
using PersonalFinanceTracker.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _repoMock = new();
    private readonly IMapper _mapper;

    public AccountServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    private AccountService CreateSut() => new(_repoMock.Object, _mapper);

    [Fact]
    public async Task GetUserAccountsAsync_ReturnsAllAccountsForUser()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = 1, UserId = 10, Name = "Checking", Type = AccountType.Checking, Balance = 1500, Currency = "USD" },
            new() { Id = 2, UserId = 10, Name = "Savings",  Type = AccountType.Savings,  Balance = 5000, Currency = "USD" }
        };
        _repoMock.Setup(r => r.GetByUserIdAsync(10)).ReturnsAsync(accounts);

        var sut = CreateSut();

        // Act
        var result = await sut.GetUserAccountsAsync(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnerRequests_ReturnsAccount()
    {
        // Arrange
        var account = new Account { Id = 5, UserId = 10, Name = "Cash", Type = AccountType.Cash, Balance = 200, Currency = "USD" };
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(5, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Cash");
    }

    [Fact]
    public async Task GetByIdAsync_WhenDifferentUserRequests_Returns404()
    {
        // Arrange — account belongs to user 10, user 99 is requesting
        var account = new Account { Id = 5, UserId = 10, Name = "Cash", Type = AccountType.Cash, Balance = 200 };
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(5, 99);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateAsync_WithUniqueName_ReturnsCreatedAccount()
    {
        // Arrange
        _repoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
                 .ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Account>()))
                 .ReturnsAsync((Account a) => { a.Id = 1; return a; });

        var sut = CreateSut();
        var dto = new CreateAccountDto { Name = "My Wallet", Type = AccountType.Cash, Balance = 500, Currency = "EUR" };

        // Act
        var result = await sut.CreateAsync(dto, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.Name.Should().Be("My Wallet");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_Returns409()
    {
        // Arrange
        _repoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
                 .ReturnsAsync(true);

        var sut = CreateSut();
        var dto = new CreateAccountDto { Name = "Existing", Type = AccountType.Checking, Balance = 0 };

        // Act
        var result = await sut.CreateAsync(dto, 10);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwner_ReturnsSuccess()
    {
        // Arrange
        var account = new Account { Id = 3, UserId = 10 };
        _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(account);
        _repoMock.Setup(r => r.DeleteAsync(account)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(3, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(account), Times.Once);
    }
}
