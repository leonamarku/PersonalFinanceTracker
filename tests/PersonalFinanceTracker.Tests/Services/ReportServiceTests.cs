using Moq;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<ITransactionRepository> _txRepoMock     = new();
    private readonly Mock<IBudgetRepository>      _budgetRepoMock = new();

    private ReportService CreateSut() => new(_txRepoMock.Object, _budgetRepoMock.Object);

    // ── Shared test data ──────────────────────────────────────────────────────
    private static Category FoodCat   => new() { Id = 3, Name = "Food & Dining",  Color = "#f97316", Type = TransactionType.Expense };
    private static Category SalaryCat => new() { Id = 1, Name = "Salary",         Color = "#22c55e", Type = TransactionType.Income };
    private static Account  UserAcc   => new() { Id = 1, UserId = 10, Name = "Main", Balance = 1000, Currency = "USD" };

    private static List<Transaction> BuildTransactions() => new()
    {
        new() { Id = 1, AccountId = 1, CategoryId = 1, Category = SalaryCat, Account = UserAcc,
                Amount = 3000m, Type = TransactionType.Income,  Date = new DateTime(2026, 6, 1) },
        new() { Id = 2, AccountId = 1, CategoryId = 3, Category = FoodCat,   Account = UserAcc,
                Amount = 450m,  Type = TransactionType.Expense, Date = new DateTime(2026, 6, 5) },
        new() { Id = 3, AccountId = 1, CategoryId = 3, Category = FoodCat,   Account = UserAcc,
                Amount = 200m,  Type = TransactionType.Expense, Date = new DateTime(2026, 6, 15) }
    };

    // ── MonthlySummary ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMonthlySummary_CalculatesTotalsCorrectly()
    {
        // Arrange
        _txRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                   .ReturnsAsync(BuildTransactions());

        // Act
        var result = await CreateSut().GetMonthlySummaryAsync(10, 6, 2026);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalIncome.Should().Be(3000m);
        result.Data.TotalExpenses.Should().Be(650m);  // 450 + 200
        result.Data.NetBalance.Should().Be(2350m);     // 3000 - 650
        result.Data.TransactionCount.Should().Be(3);
    }

    [Fact]
    public async Task GetMonthlySummary_WithNoTransactions_ReturnsZeroes()
    {
        // Arrange
        _txRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 1, 2026))
                   .ReturnsAsync(new List<Transaction>());

        // Act
        var result = await CreateSut().GetMonthlySummaryAsync(10, 1, 2026);

        // Assert
        result.Data!.TotalIncome.Should().Be(0);
        result.Data.TotalExpenses.Should().Be(0);
        result.Data.NetBalance.Should().Be(0);
    }

    // ── CategoryBreakdown ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategoryBreakdown_GroupsExpensesByCategory()
    {
        // Arrange
        _txRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                   .ReturnsAsync(BuildTransactions());

        // Act
        var result = await CreateSut().GetCategoryBreakdownAsync(10, 6, 2026);

        // Assert — only expenses group; Food total = 650 = 100%
        result.IsSuccess.Should().BeTrue();
        var items = result.Data!.ToList();
        items.Should().HaveCount(1);
        items[0].CategoryName.Should().Be("Food & Dining");
        items[0].TotalAmount.Should().Be(650m);
        items[0].Percentage.Should().Be(100m);
        items[0].Count.Should().Be(2);
    }

    // ── BudgetStatus ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBudgetStatus_WhenOverBudget_SetsIsOverBudgetTrue()
    {
        // Arrange
        var budget = new Budget { Id = 1, UserId = 10, CategoryId = 3, Month = 6, Year = 2026,
                                  LimitAmount = 500m, Category = FoodCat };
        _budgetRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                       .ReturnsAsync(new List<Budget> { budget });
        _txRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                   .ReturnsAsync(BuildTransactions());

        // Act
        var result = await CreateSut().GetBudgetStatusAsync(10, 6, 2026);

        // Assert — spent 650, limit 500 → over budget
        result.IsSuccess.Should().BeTrue();
        var status = result.Data!.First();
        status.SpentAmount.Should().Be(650m);
        status.LimitAmount.Should().Be(500m);
        status.IsOverBudget.Should().BeTrue();
        status.RemainingAmount.Should().Be(-150m);
        status.PercentUsed.Should().Be(130m);
    }

    [Fact]
    public async Task GetBudgetStatus_WhenUnderBudget_SetsIsOverBudgetFalse()
    {
        // Arrange — budget of 1000, spent 650
        var budget = new Budget { Id = 2, UserId = 10, CategoryId = 3, Month = 6, Year = 2026,
                                  LimitAmount = 1000m, Category = FoodCat };
        _budgetRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                       .ReturnsAsync(new List<Budget> { budget });
        _txRepoMock.Setup(r => r.GetByUserAndMonthAsync(10, 6, 2026))
                   .ReturnsAsync(BuildTransactions());

        // Act
        var result = await CreateSut().GetBudgetStatusAsync(10, 6, 2026);

        // Assert
        result.Data!.First().IsOverBudget.Should().BeFalse();
        result.Data.First().RemainingAmount.Should().Be(350m);
    }
}
