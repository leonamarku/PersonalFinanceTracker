using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Infrastructure.Data;
using PersonalFinanceTracker.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;

namespace PersonalFinanceTracker.Tests.Repositories;

/// <summary>
/// Repository tests use EF Core InMemory provider so no real database is needed.
/// Each test gets a fresh context via a unique database name.
/// </summary>
public class AccountRepositoryTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsAccount()
    {
        // Arrange
        using var ctx  = CreateContext(nameof(AddAsync_PersistsAccount));
        var repo       = new AccountRepository(ctx);
        var account    = new Account { UserId = 1, Name = "Savings", Type = AccountType.Savings,
                                       Balance = 1000m, Currency = "USD" };

        // Act
        await repo.AddAsync(account);

        // Assert
        ctx.Accounts.Should().HaveCount(1);
        ctx.Accounts.First().Name.Should().Be("Savings");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUserAccounts()
    {
        // Arrange
        using var ctx = CreateContext(nameof(GetByUserIdAsync_ReturnsOnlyUserAccounts));
        ctx.Accounts.AddRange(
            new Account { UserId = 1, Name = "A1", Type = AccountType.Cash,     Balance = 0 },
            new Account { UserId = 1, Name = "A2", Type = AccountType.Checking, Balance = 0 },
            new Account { UserId = 2, Name = "B1", Type = AccountType.Savings,  Balance = 0 }
        );
        await ctx.SaveChangesAsync();

        var repo = new AccountRepository(ctx);

        // Act
        var result = (await repo.GetByUserIdAsync(1)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.UserId.Should().Be(1));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsAccount()
    {
        // Arrange
        using var ctx = CreateContext(nameof(GetByIdAsync_WhenExists_ReturnsAccount));
        var account   = new Account { UserId = 1, Name = "Checking", Type = AccountType.Checking, Balance = 500 };
        ctx.Accounts.Add(account);
        await ctx.SaveChangesAsync();

        var repo = new AccountRepository(ctx);

        // Act
        var found = await repo.GetByIdAsync(account.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Checking");
    }

    [Fact]
    public async Task DeleteAsync_RemovesAccountFromDatabase()
    {
        // Arrange
        using var ctx = CreateContext(nameof(DeleteAsync_RemovesAccountFromDatabase));
        var account   = new Account { UserId = 1, Name = "ToDelete", Type = AccountType.Cash, Balance = 0 };
        ctx.Accounts.Add(account);
        await ctx.SaveChangesAsync();

        var repo = new AccountRepository(ctx);

        // Act
        await repo.DeleteAsync(account);

        // Assert
        ctx.Accounts.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsAsync_WhenAccountExists_ReturnsTrue()
    {
        // Arrange
        using var ctx = CreateContext(nameof(ExistsAsync_WhenAccountExists_ReturnsTrue));
        ctx.Accounts.Add(new Account { UserId = 1, Name = "Wallet", Type = AccountType.Cash, Balance = 0 });
        await ctx.SaveChangesAsync();

        var repo = new AccountRepository(ctx);

        // Act
        var exists = await repo.ExistsAsync(a => a.Name == "Wallet" && a.UserId == 1);

        // Assert
        exists.Should().BeTrue();
    }
}
