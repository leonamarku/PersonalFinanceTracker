using AutoMapper;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Category;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IMapper             _mapper;

    public CategoryService(ICategoryRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetVisibleAsync(int userId)
    {
        var cats = await _repo.GetVisibleToUserAsync(userId);
        return Result<IEnumerable<CategoryDto>>.Success(_mapper.Map<IEnumerable<CategoryDto>>(cats));
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(int id, int userId)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null || (!cat.IsGlobal && cat.UserId != userId))
            return Result<CategoryDto>.Failure("Category not found.", 404);

        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(cat));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, int userId, bool isAdmin)
    {
        // Only admins can create global categories
        if (dto.IsGlobal && !isAdmin)
            return Result<CategoryDto>.Failure("Only administrators can create global categories.", 403);

        var cat = _mapper.Map<Category>(dto);
        cat.UserId   = dto.IsGlobal ? null : userId;
        cat.IsGlobal = dto.IsGlobal && isAdmin;

        await _repo.AddAsync(cat);
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(cat), 201);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, int userId, bool isAdmin)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return Result<CategoryDto>.Failure("Category not found.", 404);

        // Global categories can only be edited by admins; personal by their owner
        if (cat.IsGlobal && !isAdmin)
            return Result<CategoryDto>.Failure("Only administrators can edit global categories.", 403);
        if (!cat.IsGlobal && cat.UserId != userId)
            return Result<CategoryDto>.Failure("Category not found.", 404);

        cat.Name  = dto.Name;
        cat.Type  = dto.Type;
        cat.Color = dto.Color;

        await _repo.UpdateAsync(cat);
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(cat));
    }

    public async Task<Result<bool>> DeleteAsync(int id, int userId, bool isAdmin)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return Result<bool>.Failure("Category not found.", 404);

        if (cat.IsGlobal && !isAdmin)
            return Result<bool>.Failure("Only administrators can delete global categories.", 403);
        if (!cat.IsGlobal && cat.UserId != userId)
            return Result<bool>.Failure("Category not found.", 404);

        await _repo.DeleteAsync(cat);
        return Result<bool>.Success(true);
    }
}
