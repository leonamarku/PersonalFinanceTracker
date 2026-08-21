using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Interfaces.Repositories;

/// <summary>Account-specific data access operations.</summary>
public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
    Task<Account?> GetByIdWithTransactionsAsync(int accountId);
}
