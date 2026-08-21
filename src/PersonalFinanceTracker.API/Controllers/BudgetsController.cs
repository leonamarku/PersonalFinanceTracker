using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Budget;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>CRUD for monthly spending budgets per category.</summary>
[Route("api/budgets")]
public class BudgetsController : ApiControllerBase
{
    private readonly IBudgetService _service;

    public BudgetsController(IBudgetService service) => _service = service;

    /// <summary>Get all budgets for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetUserBudgetsAsync(CurrentUserId);
        return Ok(result.Data);
    }

    /// <summary>Get budgets for a specific month.</summary>
    [HttpGet("month/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(IEnumerable<BudgetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMonth(int year, int month)
    {
        var result = await _service.GetByMonthAsync(CurrentUserId, month, year);
        return Ok(result.Data);
    }

    /// <summary>Get a single budget by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Set a new monthly budget for a category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Update the spending limit of an existing budget.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Delete a budget.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }
}
