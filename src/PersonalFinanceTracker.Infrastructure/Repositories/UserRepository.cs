using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Data;

namespace PersonalFinanceTracker.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _dbSet.OrderBy(u => u.FullName).ToListAsync();
}
