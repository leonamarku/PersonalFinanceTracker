using PersonalFinanceTracker.Core.Common;
using PersonalFinanceTracker.Core.DTOs.Auth;

namespace PersonalFinanceTracker.Core.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
}
