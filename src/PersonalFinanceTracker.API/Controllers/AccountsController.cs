using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Account;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>CRUD for financial accounts owned by the authenticated user.</summary>
[Route("api/accounts")]
public class AccountsController : ApiControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service) => _service = service;

    /// <summary>Get all accounts belonging to the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetUserAccountsAsync(CurrentUserId);
        return Ok(result.Data);
    }

    /// <summary>Get a single account by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Create a new account.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Update an existing account.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Delete an account and all its transactions.</summary>
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
