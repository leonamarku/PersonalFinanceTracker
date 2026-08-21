using AutoMapper;
using Moq;
using PersonalFinanceTracker.Core.DTOs.Transaction;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Mappings;
using PersonalFinanceTracker.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _txRepoMock       = new();
    private readonly Mock<IAccountRepository>     _accountRepoMock  = new();
    private readonly Mock<ICategoryRepository>    _categoryRepoMock = new();
    private readonly IMapper _mapper;

    public TransactionServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    private TransactionService CreateSut() =>
        new(_txRepoMock.Object, _accountRepoMock.Object, _categoryRepoMock.Object, _mapper);

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Account UserAccount(decimal balance = 2000m) =>
        new() { Id = 1, UserId = 10, Name = "Main", Balance = balance, Currency = "USD" };

    private static Category ExpenseCat =>
        new() { Id = 3, Name = "Food", Color = "#f97316", Type = TransactionType.Expense, IsGlobal = true };

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidExpense_DeductsFromAccountBalance()
    {
        // Arrange
        var account  = UserAccount(2000m);
        var category = ExpenseCat;

        _accountRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(category);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                   .ReturnsAsync((Transaction t) => { t.Id = 99; t.Account = account; t.Category = category; return t; });
        _txRepoMock.Setup(r => r.GetByIdWithDetailsAsync(99))
                   .ReturnsAsync(new Transaction { Id = 99, AccountId = 1, CategoryId = 3,
                       Amount = 300m, Type = TransactionType.Expense,
                       Date = DateTime.Today, Description = "Lunch",
                       Account = account, Category = category });
        _accountRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

        var dto = new CreateTransactionDto
        {
            AccountId = 1, CategoryId = 3, Amount = 300m,
            Type = TransactionType.Expense, Date = DateTime.Today, Description = "Lunch"
        };

        // Act
        var result = await CreateSut().CreateAsync(dto, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        // Balance should be reduced by 300
        account.Balance.Should().Be(1700m);
        _accountRepoMock.Verify(r => r.UpdateAsync(account), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ExpenseExceedsBalance_Returns422()
    {
        // Arrange — balance 100, trying to spend 500
        var account  = UserAccount(100m);
        var category = ExpenseCat;

        _accountRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(category);

        var dto = new CreateTransactionDto
        {
            AccountId = 1, CategoryId = 3, Amount = 500m,
            Type = TransactionType.Expense, Date = DateTime.Today, Description = "Too big"
        };

        // Act
        var result = await CreateSut().CreateAsync(dto, 10);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(422);
        result.Error.Should().Contain("Insufficient balance");
        _txRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_AccountBelongsToDifferentUser_Returns404()
    {
        // Arrange — account.UserId = 10, requesting userId = 99
        var account = new Account { Id = 1, UserId = 10, Balance = 1000 };
        _accountRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 3, Amount = 100m,
            Type = TransactionType.Expense, Date = DateTime.Today, Description = "Test" };

        var result = await CreateSut().CreateAsync(dto, 99);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateAsync_IncomeTransaction_IncreasesAccountBalance()
    {
        // Arrange
        var account  = UserAccount(1000m);
        var incomeCat = new Category { Id = 1, Name = "Salary", IsGlobal = true, Type = TransactionType.Income };

        _accountRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incomeCat);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                   .ReturnsAsync((Transaction t) => { t.Id = 50; t.Account = account; t.Category = incomeCat; return t; });
        _txRepoMock.Setup(r => r.GetByIdWithDetailsAsync(50))
                   .ReturnsAsync(new Transaction { Id = 50, AccountId = 1, CategoryId = 1,
                       Amount = 2000m, Type = TransactionType.Income, Date = DateTime.Today,
                       Description = "Salary June", Account = account, Category = incomeCat });
        _accountRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);

        var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 2000m,
            Type = TransactionType.Income, Date = DateTime.Today, Description = "Salary June" };

        // Act
        var result = await CreateSut().CreateAsync(dto, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(3000m); // 1000 + 2000
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidTransaction_ReversesBalanceAndDeletes()
    {
        // Arrange
        var account = UserAccount(1700m); // after a 300 expense from 2000
        var tx = new Transaction { Id = 10, AccountId = 1, CategoryId = 3, Amount = 300m,
            Type = TransactionType.Expense, Date = DateTime.Today, Account = account,
            Category = ExpenseCat, Description = "Lunch" };

        _txRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(tx);
        _accountRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _txRepoMock.Setup(r => r.DeleteAsync(tx)).Returns(Task.CompletedTask);

        // Act
        var result = await CreateSut().DeleteAsync(10, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(2000m); // reversed: 1700 + 300
        _txRepoMock.Verify(r => r.DeleteAsync(tx), Times.Once);
    }
}
