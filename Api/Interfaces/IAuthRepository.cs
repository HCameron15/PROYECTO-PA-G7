using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.Auth;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IAuthRepository
{
    Task<ApiOperationResultDto<LoginResponseDto>> LoginAsync(LoginRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<VerifyOtpResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<VerifyOtpResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> LogoutAsync(LogoutRequestDto resource, CancellationToken cancellationToken = default);
}