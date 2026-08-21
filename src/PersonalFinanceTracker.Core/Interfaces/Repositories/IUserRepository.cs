using PersonalFinanceTracker.Core.Entities;

namespace PersonalFinanceTracker.Core.Interfaces.Repositories;

/// <summary>User-specific data access operations.</summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllUsersAsync();
}
