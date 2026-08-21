using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Data;

namespace PersonalFinanceTracker.Infrastructure.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
        => await _dbSet.Where(a => a.UserId == userId)
                       .OrderBy(a => a.Name)
                       .ToListAsync();

    public async Task<Account?> GetByIdWithTransactionsAsync(int accountId)
        => await _dbSet.Include(a => a.Transactions)
                       .ThenInclude(t => t.Category)
                       .FirstOrDefaultAsync(a => a.Id == accountId);
}
