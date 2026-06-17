using Uam.AdvancedProgramming.Api.DTOs;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IAuthRepository
{
    Task<ApiOperationResultDto<LoginResponseDto>> LoginAsync(LoginRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> LogoutAsync(LogoutRequestDto resource, CancellationToken cancellationToken = default);
}