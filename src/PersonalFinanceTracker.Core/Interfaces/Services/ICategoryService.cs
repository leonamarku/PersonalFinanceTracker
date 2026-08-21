using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Category;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetVisibleAsync(int userId);
    Task<Result<CategoryDto>>              GetByIdAsync(int id, int userId);
    Task<Result<CategoryDto>>              CreateAsync(CreateCategoryDto dto, int userId, bool isAdmin);
    Task<Result<CategoryDto>>              UpdateAsync(int id, UpdateCategoryDto dto, int userId, bool isAdmin);
    Task<Result<bool>>                     DeleteAsync(int id, int userId, bool isAdmin);
}
