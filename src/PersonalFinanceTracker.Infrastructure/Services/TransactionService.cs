using AutoMapper;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Transaction;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

/// <summary>
/// Manages transaction CRUD and the inter-account transfer workflow.
/// Every write operation automatically recalculates account balances so
/// they always reflect the true sum of posted transactions.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _txRepo;
    private readonly IAccountRepository     _accountRepo;
    private readonly ICategoryRepository    _categoryRepo;
    private readonly IMapper                _mapper;

    public TransactionService(
        ITransactionRepository txRepo,
        IAccountRepository     accountRepo,
        ICategoryRepository    categoryRepo,
        IMapper                mapper)
    {
        _txRepo       = txRepo;
        _accountRepo  = accountRepo;
        _categoryRepo = categoryRepo;
        _mapper       = mapper;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<TransactionDto>>> GetUserTransactionsAsync(int userId)
    {
        var txs = await _txRepo.GetByUserIdAsync(userId);
        return Result<IEnumerable<TransactionDto>>.Success(_mapper.Map<IEnumerable<TransactionDto>>(txs));
    }

    public async Task<Result<IEnumerable<TransactionDto>>> GetByAccountAsync(int accountId, int userId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account is null || account.UserId != userId)
            return Result<IEnumerable<TransactionDto>>.Failure("Account not found.", 404);

        var txs = await _txRepo.GetByAccountIdAsync(accountId);
        return Result<IEnumerable<TransactionDto>>.Success(_mapper.Map<IEnumerable<TransactionDto>>(txs));
    }

    public async Task<Result<TransactionDto>> GetByIdAsync(int id, int userId)
    {
        var tx = await _txRepo.GetByIdWithDetailsAsync(id);
        if (tx is null || tx.Account.UserId != userId)
            return Result<TransactionDto>.Failure("Transaction not found.", 404);

        return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<Result<TransactionDto>> CreateAsync(CreateTransactionDto dto, int userId)
    {
        // Validate account ownership
        var account = await _accountRepo.GetByIdAsync(dto.AccountId);
        if (account is null || account.UserId != userId)
            return Result<TransactionDto>.Failure("Account not found.", 404);

        // Validate category visibility
        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
        if (category is null || (!category.IsGlobal && category.UserId != userId))
            return Result<TransactionDto>.Failure("Category not found.", 404);

        // Business rule: expense cannot exceed account balance
        if (dto.Type == TransactionType.Expense && dto.Amount > account.Balance)
            return Result<TransactionDto>.Failure(
                $"Insufficient balance. Available: {account.Balance:F2} {account.Currency}.", 422);

        var tx = _mapper.Map<Transaction>(dto);
        tx.CreatedAt = DateTime.UtcNow;
        await _txRepo.AddAsync(tx);

        // Recalculate balance
        await AdjustBalanceAsync(account, dto.Type, dto.Amount);

        // Reload with navigation properties for the DTO
        var created = await _txRepo.GetByIdWithDetailsAsync(tx.Id);
        return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(created!), 201);
    }

    public async Task<Result<TransactionDto>> UpdateAsync(int id, UpdateTransactionDto dto, int userId)
    {
        var tx = await _txRepo.GetByIdWithDetailsAsync(id);
        if (tx is null || tx.Account.UserId != userId)
            return Result<TransactionDto>.Failure("Transaction not found.", 404);

        // Cannot edit a transfer leg directly
        if (tx.TransferPairId.HasValue)
            return Result<TransactionDto>.Failure(
                "Transfer transactions cannot be edited individually. Delete and recreate the transfer.", 422);

        // Validate category
        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
        if (category is null || (!category.IsGlobal && category.UserId != userId))
            return Result<TransactionDto>.Failure("Category not found.", 404);

        // Reverse the old effect on balance, then apply the new one
        var account = tx.Account;
        await ReverseBalanceAsync(account, tx.Type, tx.Amount);

        if (dto.Type == TransactionType.Expense && dto.Amount > account.Balance)
        {
            // Revert the reversal before returning the error
            await AdjustBalanceAsync(account, tx.Type, tx.Amount);
            return Result<TransactionDto>.Failure(
                $"Insufficient balance after update. Available: {account.Balance:F2} {account.Currency}.", 422);
        }

        tx.CategoryId   = dto.CategoryId;
        tx.Amount       = dto.Amount;
        tx.Type         = dto.Type;
        tx.Date         = dto.Date;
        tx.Description  = dto.Description;
        tx.Notes        = dto.Notes;

        await _txRepo.UpdateAsync(tx);
        await AdjustBalanceAsync(account, dto.Type, dto.Amount);

        var updated = await _txRepo.GetByIdWithDetailsAsync(tx.Id);
        return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(updated!));
    }

    public async Task<Result<bool>> DeleteAsync(int id, int userId)
    {
        var tx = await _txRepo.GetByIdWithDetailsAsync(id);
        if (tx is null || tx.Account.UserId != userId)
            return Result<bool>.Failure("Transaction not found.", 404);

        // If this is a transfer leg, delete both legs atomically
        if (tx.TransferPairId.HasValue)
        {
            var pair = await _txRepo.GetByIdWithDetailsAsync(tx.TransferPairId.Value);
            if (pair is not null)
            {
                await ReverseBalanceAsync(pair.Account, pair.Type, pair.Amount);
                await _txRepo.DeleteAsync(pair);
            }
        }

        var account = tx.Account;
        await ReverseBalanceAsync(account, tx.Type, tx.Amount);
        await _txRepo.DeleteAsync(tx);

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Creates two linked transactions: an Expense on the source account
    /// and an Income on the destination account, sharing a TransferPairId.
    /// </summary>
    public async Task<Result<TransferResultDto>> TransferAsync(TransferDto dto, int userId)
    {
        if (dto.FromAccountId == dto.ToAccountId)
            return Result<TransferResultDto>.Failure("Source and destination accounts must be different.", 422);

        var from = await _accountRepo.GetByIdAsync(dto.FromAccountId);
        var to   = await _accountRepo.GetByIdAsync(dto.ToAccountId);

        if (from is null || from.UserId != userId)
            return Result<TransferResultDto>.Failure("Source account not found.", 404);
        if (to is null || to.UserId != userId)
            return Result<TransferResultDto>.Failure("Destination account not found.", 404);

        if (dto.Amount > from.Balance)
            return Result<TransferResultDto>.Failure(
                $"Insufficient balance. Available: {from.Balance:F2} {from.Currency}.", 422);

        // Use the seeded "Transfer" category (we use category ID 10 as a placeholder;
        // in production the seed would include a Transfer category)
        // We create both legs with a shared TransferPairId (the ID of the second leg + 1;
        // simpler: set it after both are saved)
        var now = DateTime.UtcNow;

        // Find or fallback to first available category
        var categories = await _categoryRepo.GetVisibleToUserAsync(userId);
        var transferCategoryId = categories.FirstOrDefault()?.Id ?? 1;

        var outgoing = new Transaction
        {
            AccountId    = dto.FromAccountId,
            CategoryId   = transferCategoryId,
            Amount       = dto.Amount,
            Type         = TransactionType.Expense,
            Date         = dto.Date,
            Description  = $"Transfer to {to.Name}: {dto.Description}",
            Notes        = dto.Notes,
            CreatedAt    = now
        };
        var incoming = new Transaction
        {
            AccountId    = dto.ToAccountId,
            CategoryId   = transferCategoryId,
            Amount       = dto.Amount,
            Type         = TransactionType.Income,
            Date         = dto.Date,
            Description  = $"Transfer from {from.Name}: {dto.Description}",
            Notes        = dto.Notes,
            CreatedAt    = now
        };

        await _txRepo.AddAsync(outgoing);
        await _txRepo.AddAsync(incoming);

        // Link the two legs
        outgoing.TransferPairId = incoming.Id;
        incoming.TransferPairId = outgoing.Id;
        await _txRepo.UpdateAsync(outgoing);
        await _txRepo.UpdateAsync(incoming);

        // Adjust balances
        from.Balance -= dto.Amount;
        to.Balance   += dto.Amount;
        await _accountRepo.UpdateAsync(from);
        await _accountRepo.UpdateAsync(to);

        var outDto = await _txRepo.GetByIdWithDetailsAsync(outgoing.Id);
        var inDto  = await _txRepo.GetByIdWithDetailsAsync(incoming.Id);

        return Result<TransferResultDto>.Success(new TransferResultDto
        {
            Outgoing = _mapper.Map<TransactionDto>(outDto!),
            Incoming = _mapper.Map<TransactionDto>(inDto!)
        }, 201);
    }

    // ── Balance helpers ───────────────────────────────────────────────────────

    private async Task AdjustBalanceAsync(Account account, TransactionType type, decimal amount)
    {
        account.Balance += type == TransactionType.Income ? amount : -amount;
        await _accountRepo.UpdateAsync(account);
    }

    private async Task ReverseBalanceAsync(Account account, TransactionType type, decimal amount)
    {
        // Opposite of AdjustBalance
        account.Balance += type == TransactionType.Income ? -amount : amount;
        await _accountRepo.UpdateAsync(account);
    }
}
