using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Interfaces.Repositories;

/// <summary>Category-specific data access operations.</summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>Returns global categories plus categories owned by the given user.</summary>
    Task<IEnumerable<Category>> GetVisibleToUserAsync(int userId);
    Task<IEnumerable<Category>> GetGlobalCategoriesAsync();
}
