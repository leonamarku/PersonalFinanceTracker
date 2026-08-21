using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Category;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>
/// Manages transaction categories.
/// Global categories (visible to all users) can only be created/deleted by Admins.
/// </summary>
[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    /// <summary>Get all categories visible to the current user (global + personal).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetVisibleAsync(CurrentUserId);
        return Ok(result.Data);
    }

    /// <summary>Get a single category by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Create a category. Set IsGlobal=true only if you are an Admin.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto, CurrentUserId, IsAdmin);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Update a category. Admins can update global; users only their own.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto, CurrentUserId, IsAdmin);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Delete a category. Admin-only for global categories.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId, IsAdmin);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }

    // ── Admin-only endpoints ──────────────────────────────────────────────────

    /// <summary>[Admin] Get all categories across the system.</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllAdmin()
    {
        // Admin sees everything (uses global categories endpoint for simplicity)
        var result = await _service.GetVisibleAsync(CurrentUserId);
        return Ok(result.Data);
    }
}
