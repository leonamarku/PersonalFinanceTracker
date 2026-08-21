using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Core.DTOs.Transaction;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.API.Controllers;

/// <summary>
/// Full CRUD for transactions plus the inter-account transfer operation.
/// All endpoints are scoped to the authenticated user's own data.
/// </summary>
[Route("api/transactions")]
public class TransactionsController : ApiControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service) => _service = service;

    /// <summary>Get all transactions for the current user, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetUserTransactionsAsync(CurrentUserId);
        return Ok(result.Data);
    }

    /// <summary>Get all transactions for a specific account.</summary>
    [HttpGet("account/{accountId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAccount(int accountId)
    {
        var result = await _service.GetByAccountAsync(accountId, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Get a single transaction by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new transaction. Also adjusts the account balance automatically.
    /// Business rule: an expense cannot exceed the available account balance.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>Update a transaction. Balance is recalculated automatically.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>Delete a transaction. Balance is reversed automatically.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Transfer money between two of the user's accounts.
    /// Creates two linked transactions: an expense on the source and an income on the destination.
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.TransferAsync(dto, CurrentUserId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(StatusCodes.Status201Created, result.Data);
    }
}
