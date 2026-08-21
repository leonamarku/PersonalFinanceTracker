using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Interfaces.Repositories;

/// <summary>Transaction-specific data access operations.</summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Transaction>> GetByUserAndMonthAsync(int userId, int month, int year);
    Task<IEnumerable<Transaction>> GetByCategoryAndMonthAsync(int userId, int categoryId, int month, int year);
    Task<Transaction?> GetByIdWithDetailsAsync(int id);
}
