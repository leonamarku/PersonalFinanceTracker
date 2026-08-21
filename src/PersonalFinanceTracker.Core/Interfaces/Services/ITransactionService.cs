using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Transaction;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface ITransactionService
{
    Task<Result<IEnumerable<TransactionDto>>> GetUserTransactionsAsync(int userId);
    Task<Result<IEnumerable<TransactionDto>>> GetByAccountAsync(int accountId, int userId);
    Task<Result<TransactionDto>>              GetByIdAsync(int id, int userId);
    Task<Result<TransactionDto>>              CreateAsync(CreateTransactionDto dto, int userId);
    Task<Result<TransactionDto>>              UpdateAsync(int id, UpdateTransactionDto dto, int userId);
    Task<Result<bool>>                        DeleteAsync(int id, int userId);
    /// <summary>Creates two linked transactions representing a transfer between accounts.</summary>
    Task<Result<TransferResultDto>>           TransferAsync(TransferDto dto, int userId);
}
