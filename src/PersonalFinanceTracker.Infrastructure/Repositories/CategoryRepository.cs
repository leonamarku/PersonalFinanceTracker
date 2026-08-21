using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Infrastructure.Data;

namespace PersonalFinanceTracker.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetVisibleToUserAsync(int userId)
        => await _dbSet.Where(c => c.IsGlobal || c.UserId == userId)
                       .OrderBy(c => c.Name)
                       .ToListAsync();

    public async Task<IEnumerable<Category>> GetGlobalCategoriesAsync()
        => await _dbSet.Where(c => c.IsGlobal)
                       .OrderBy(c => c.Name)
                       .ToListAsync();
}
