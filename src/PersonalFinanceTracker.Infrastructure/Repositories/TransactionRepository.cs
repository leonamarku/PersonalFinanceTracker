using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Data;

namespace PersonalFinanceTracker.Infrastructure.Repositories;

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId)
        => await _dbSet.Include(t => t.Category)
                       .Include(t => t.Account)
                       .Where(t => t.AccountId == accountId)
                       .OrderByDescending(t => t.Date)
                       .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
        => await _dbSet.Include(t => t.Category)
                       .Include(t => t.Account)
                       .Where(t => t.Account.UserId == userId)
                       .OrderByDescending(t => t.Date)
                       .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByUserAndMonthAsync(int userId, int month, int year)
        => await _dbSet.Include(t => t.Category)
                       .Include(t => t.Account)
                       .Where(t => t.Account.UserId == userId
                                && t.Date.Month == month
                                && t.Date.Year  == year)
                       .OrderByDescending(t => t.Date)
                       .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByCategoryAndMonthAsync(
        int userId, int categoryId, int month, int year)
        => await _dbSet.Include(t => t.Account)
                       .Where(t => t.Account.UserId == userId
                                && t.CategoryId   == categoryId
                                && t.Date.Month   == month
                                && t.Date.Year    == year)
                       .ToListAsync();

    public async Task<Transaction?> GetByIdWithDetailsAsync(int id)
        => await _dbSet.Include(t => t.Category)
                       .Include(t => t.Account)
                       .FirstOrDefaultAsync(t => t.Id == id);
}
