using AutoMapper;
using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Account;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Interfaces.Repositories;
using PersonalFinanceTracker.Core.Interfaces.Services;

namespace PersonalFinanceTracker.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repo;
    private readonly IMapper            _mapper;

    public AccountService(IAccountRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AccountDto>>> GetUserAccountsAsync(int userId)
    {
        var accounts = await _repo.GetByUserIdAsync(userId);
        return Result<IEnumerable<AccountDto>>.Success(_mapper.Map<IEnumerable<AccountDto>>(accounts));
    }

    public async Task<Result<AccountDto>> GetByIdAsync(int id, int userId)
    {
        var account = await _repo.GetByIdAsync(id);
        if (account is null || account.UserId != userId)
            return Result<AccountDto>.Failure("Account not found.", 404);

        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public async Task<Result<AccountDto>> CreateAsync(CreateAccountDto dto, int userId)
    {
        // Business rule: user may not have two accounts with the same name
        var duplicate = await _repo.ExistsAsync(a => a.UserId == userId &&
                                                     a.Name.ToLower() == dto.Name.ToLower());
        if (duplicate)
            return Result<AccountDto>.Failure("An account with this name already exists.", 409);

        var account = _mapper.Map<Account>(dto);
        account.UserId    = userId;
        account.CreatedAt = DateTime.UtcNow;

        await _repo.AddAsync(account);
        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account), 201);
    }

    public async Task<Result<AccountDto>> UpdateAsync(int id, UpdateAccountDto dto, int userId)
    {
        var account = await _repo.GetByIdAsync(id);
        if (account is null || account.UserId != userId)
            return Result<AccountDto>.Failure("Account not found.", 404);

        // Business rule: no duplicate name on update either
        var duplicate = await _repo.ExistsAsync(a => a.UserId == userId &&
                                                     a.Name.ToLower() == dto.Name.ToLower() &&
                                                     a.Id != id);
        if (duplicate)
            return Result<AccountDto>.Failure("Another account with this name already exists.", 409);

        account.Name     = dto.Name;
        account.Type     = dto.Type;
        account.Currency = dto.Currency;

        await _repo.UpdateAsync(account);
        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }

    public async Task<Result<bool>> DeleteAsync(int id, int userId)
    {
        var account = await _repo.GetByIdAsync(id);
        if (account is null || account.UserId != userId)
            return Result<bool>.Failure("Account not found.", 404);

        await _repo.DeleteAsync(account);
        return Result<bool>.Success(true);
    }
}
