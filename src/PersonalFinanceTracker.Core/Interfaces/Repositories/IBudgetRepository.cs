using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Interfaces.Repositories;

/// <summary>Budget-specific data access operations.</summary>
public interface IBudgetRepository : IRepository<Budget>
{
    Task<IEnumerable<Budget>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Budget>> GetByUserAndMonthAsync(int userId, int month, int year);
    Task<Budget?> GetByUserCategoryMonthAsync(int userId, int categoryId, int month, int year);
}
