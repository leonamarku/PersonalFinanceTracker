using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Account;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface IAccountService
{
    Task<Result<IEnumerable<AccountDto>>> GetUserAccountsAsync(int userId);
    Task<Result<AccountDto>>             GetByIdAsync(int id, int userId);
    Task<Result<AccountDto>>             CreateAsync(CreateAccountDto dto, int userId);
    Task<Result<AccountDto>>             UpdateAsync(int id, UpdateAccountDto dto, int userId);
    Task<Result<bool>>                   DeleteAsync(int id, int userId);
}
