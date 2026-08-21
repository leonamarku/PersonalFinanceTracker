using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Budget;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface IBudgetService
{
    Task<Result<IEnumerable<BudgetDto>>> GetUserBudgetsAsync(int userId);
    Task<Result<IEnumerable<BudgetDto>>> GetByMonthAsync(int userId, int month, int year);
    Task<Result<BudgetDto>>              GetByIdAsync(int id, int userId);
    Task<Result<BudgetDto>>              CreateAsync(CreateBudgetDto dto, int userId);
    Task<Result<BudgetDto>>              UpdateAsync(int id, UpdateBudgetDto dto, int userId);
    Task<Result<bool>>                   DeleteAsync(int id, int userId);
}
