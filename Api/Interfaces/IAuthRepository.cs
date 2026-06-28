using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.Auth;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IAuthRepository
{
    Task<ApiOperationResultDto<LoginResponseDto>> LoginAsync(LoginRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<VerifyOtpResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<ForgotPasswordResponseDto>> ForgotPasswordAsync(
    ForgotPasswordRequestDto resource,
    CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> ResetPasswordAsync(
        ResetPasswordRequestDto resource,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> ChangePasswordAsync(
        int userId,
        string? currentRefreshToken,
        ChangePasswordRequestDto resource,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<SessionResponseDto>>> GetMySessionsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> RevokeSessionAsync(
        int userId,
        int refreshTokenId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> RevokeAllSessionsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<VerifyOtpResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> LogoutAsync(LogoutRequestDto resource, CancellationToken cancellationToken = default);
}