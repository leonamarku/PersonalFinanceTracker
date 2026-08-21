using AutoMapper;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Budget;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository    _repo;
    private readonly ICategoryRepository  _categoryRepo;
    private readonly IMapper              _mapper;

    public BudgetService(IBudgetRepository repo, ICategoryRepository categoryRepo, IMapper mapper)
    {
        _repo         = repo;
        _categoryRepo = categoryRepo;
        _mapper       = mapper;
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetUserBudgetsAsync(int userId)
    {
        var budgets = await _repo.GetByUserIdAsync(userId);
        return Result<IEnumerable<BudgetDto>>.Success(_mapper.Map<IEnumerable<BudgetDto>>(budgets));
    }

    public async Task<Result<IEnumerable<BudgetDto>>> GetByMonthAsync(int userId, int month, int year)
    {
        var budgets = await _repo.GetByUserAndMonthAsync(userId, month, year);
        return Result<IEnumerable<BudgetDto>>.Success(_mapper.Map<IEnumerable<BudgetDto>>(budgets));
    }

    public async Task<Result<BudgetDto>> GetByIdAsync(int id, int userId)
    {
        var budget = await _repo.GetByIdAsync(id);
        if (budget is null || budget.UserId != userId)
            return Result<BudgetDto>.Failure("Budget not found.", 404);

        return Result<BudgetDto>.Success(_mapper.Map<BudgetDto>(budget));
    }

    public async Task<Result<BudgetDto>> CreateAsync(CreateBudgetDto dto, int userId)
    {
        // Validate the category is visible to this user
        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
        if (category is null || (!category.IsGlobal && category.UserId != userId))
            return Result<BudgetDto>.Failure("Category not found.", 404);

        // Business rule: only one budget per user/category/month
        var existing = await _repo.GetByUserCategoryMonthAsync(userId, dto.CategoryId, dto.Month, dto.Year);
        if (existing is not null)
            return Result<BudgetDto>.Failure(
                "A budget for this category and month already exists. Update the existing one.", 409);

        var budget = _mapper.Map<Budget>(dto);
        budget.UserId = userId;

        await _repo.AddAsync(budget);

        // Reload to include Category navigation for the DTO
        var created = await _repo.GetByIdAsync(budget.Id);
        // Manually set category since EF seed data doesn't auto-load after Add
        created!.Category = category;

        return Result<BudgetDto>.Success(_mapper.Map<BudgetDto>(created), 201);
    }

    public async Task<Result<BudgetDto>> UpdateAsync(int id, UpdateBudgetDto dto, int userId)
    {
        var budget = await _repo.GetByIdAsync(id);
        if (budget is null || budget.UserId != userId)
            return Result<BudgetDto>.Failure("Budget not found.", 404);

        budget.LimitAmount = dto.LimitAmount;
        await _repo.UpdateAsync(budget);

        // Reload with Category
        var updated = await _repo.GetByIdAsync(id);
        return Result<BudgetDto>.Success(_mapper.Map<BudgetDto>(updated!));
    }

    public async Task<Result<bool>> DeleteAsync(int id, int userId)
    {
        var budget = await _repo.GetByIdAsync(id);
        if (budget is null || budget.UserId != userId)
            return Result<bool>.Failure("Budget not found.", 404);

        await _repo.DeleteAsync(budget);
        return Result<bool>.Success(true);
    }
}
