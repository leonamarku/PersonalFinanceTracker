using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Data;

namespace PersonalFinanceTracker.Infrastructure.Repositories;

public class BudgetRepository : BaseRepository<Budget>, IBudgetRepository
{
    public BudgetRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Budget>> GetByUserIdAsync(int userId)
        => await _dbSet.Include(b => b.Category)
                       .Where(b => b.UserId == userId)
                       .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
                       .ToListAsync();

    public async Task<IEnumerable<Budget>> GetByUserAndMonthAsync(int userId, int month, int year)
        => await _dbSet.Include(b => b.Category)
                       .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
                       .ToListAsync();

    public async Task<Budget?> GetByUserCategoryMonthAsync(int userId, int categoryId, int month, int year)
        => await _dbSet.Include(b => b.Category)
                       .FirstOrDefaultAsync(b => b.UserId     == userId
                                              && b.CategoryId == categoryId
                                              && b.Month      == month
                                              && b.Year       == year);
}
