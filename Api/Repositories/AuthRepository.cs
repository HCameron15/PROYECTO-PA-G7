using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.Auth;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;
using System.Security.Cryptography;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class AuthRepository(
    AppDbContext context,
    IConfiguration configuration,
    IStringLocalizer<AuthRepository> localizer,
    IEmailService emailService
) : IAuthRepository
{
    public async Task<ApiOperationResultDto<LoginResponseDto>> LoginAsync(
    LoginRequestDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<LoginResponseDto>();
        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();

        var user = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(resource.Password, user.PasswordHash))
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidCredentials"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        var previousOtps = await context.OtpCodes
            .Where(x => x.UserId == user.Id && !x.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var item in previousOtps)
        {
            item.IsUsed = true;
        }

        var previousSessions = await context.PendingLoginSessions
            .Where(x => x.UserId == user.Id && !x.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var sessionItem in previousSessions)
        {
            sessionItem.IsUsed = true;
        }

        var otpCode = GenerateOtpCode();

        var otpExpirationMinutes = configuration.GetValue("Otp:ExpirationMinutes", 10);

        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = otpCode,
            ExpiresAtUtc = now.AddMinutes(otpExpirationMinutes),
            IsUsed = false,
            CreatedAtUtc = now
        };

        await context.OtpCodes.AddAsync(otp, cancellationToken);

        var emailSent = await emailService.SendOtpAsync(
            user.Email,
            otpCode,
            cancellationToken);

        if (!emailSent)
        {
            result.Success = false;
            result.Code = StatusCodes.Status500InternalServerError.ToString();
            result.Message = localizer["EmailSendFailed"].Value;
            return result;
        }

        var sessionExpirationMinutes = configuration.GetValue("Otp:SessionExpirationMinutes", 10);

        var session = new PendingLoginSession
        {
            UserId = user.Id,
            SessionToken = Guid.NewGuid(),
            ExpiresAtUtc = now.AddMinutes(sessionExpirationMinutes),
            IsUsed = false,
            CreatedAtUtc = now
        };

        await context.PendingLoginSessions.AddAsync(session, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["OtpSent"].Value;
        result.Result = new LoginResponseDto(session.SessionToken.ToString());

        return result;
    }

    public async Task<ApiOperationResultDto<VerifyOtpResponseDto>> VerifyOtpAsync(
    VerifyOtpRequestDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<VerifyOtpResponseDto>();

        if (!Guid.TryParse(resource.SessionToken, out var sessionToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidSession"].Value;
            return result;
        }

        var session = await context.PendingLoginSessions
            .Include(x => x.User)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.SessionToken == sessionToken,
                cancellationToken);

        if (session is null || session.IsUsed || session.ExpiresAtUtc <= DateTime.UtcNow || !session.User.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidSession"].Value;
            return result;
        }

        var otp = await context.OtpCodes
            .FirstOrDefaultAsync(
                x => x.UserId == session.UserId &&
                     x.Code == resource.Code &&
                     !x.IsUsed,
                cancellationToken);

        if (otp is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidOtp"].Value;
            return result;
        }

        if (otp.ExpiresAtUtc <= DateTime.UtcNow)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["ExpiredOtp"].Value;
            return result;
        }

        otp.IsUsed = true;
        session.IsUsed = true;

        var accessToken = GenerateAccessToken(session.User);
        var refreshToken = await CreateRefreshTokenAsync(session.UserId, cancellationToken);

        context.OtpCodes.Update(otp);
        context.PendingLoginSessions.Update(session);

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["OtpVerified"].Value;
        result.Result = new VerifyOtpResponseDto(accessToken, refreshToken.Token);

        return result;
    }

    public async Task<ApiOperationResultDto<ForgotPasswordResponseDto>> ForgotPasswordAsync(
      ForgotPasswordRequestDto resource,
      CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<ForgotPasswordResponseDto>();

        var genericMessage = localizer["PasswordRecoveryInstructionsSent"].Value;
        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail && x.IsActive, cancellationToken);

        if (user is null)
        {
            result.Success = true;
            result.Code = StatusCodes.Status200OK.ToString();
            result.Message = genericMessage;
            result.Result = new ForgotPasswordResponseDto(genericMessage, null);
            return result;
        }

        var now = DateTime.UtcNow;

        var previousRequests = await context.PasswordResetRequests
            .Where(x => x.UserId == user.Id && !x.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var request in previousRequests)
        {
            request.IsUsed = true;
            request.UsedAtUtc = now;
        }

        var code = GenerateOtpCode();

        var expirationMinutes = configuration.GetValue("PasswordReset:CodeExpirationMinutes", 10);

        var passwordResetRequest = new PasswordResetRequest
        {
            UserId = user.Id,
            SessionToken = Guid.NewGuid().ToString(),
            Code = code,
            ExpiresAtUtc = now.AddMinutes(expirationMinutes),
            IsUsed = false,
            CreatedAtUtc = now
        };

        await context.PasswordResetRequests.AddAsync(passwordResetRequest, cancellationToken);

        var emailSent = await emailService.SendOtpAsync(
            user.Email,
            code,
            cancellationToken);

        if (!emailSent)
        {
            result.Success = false;
            result.Code = StatusCodes.Status500InternalServerError.ToString();
            result.Message = localizer["EmailSendFailed"].Value;
            return result;
        }

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = genericMessage;
        result.Result = new ForgotPasswordResponseDto(
            genericMessage,
            passwordResetRequest.SessionToken);

        return result;
    }










    public async Task<ApiOperationResultDto<object>> ResetPasswordAsync(
    ResetPasswordRequestDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        if (resource.NewPassword != resource.ConfirmPassword)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["PasswordsDoNotMatch"].Value;
            return result;
        }

        var resetRequest = await context.PasswordResetRequests
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.SessionToken == resource.SessionToken &&
                     x.Code == resource.Code &&
                     !x.IsUsed,
                cancellationToken);

        if (resetRequest is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidPasswordResetRequest"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        if (resetRequest.ExpiresAtUtc <= now)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["ExpiredPasswordResetCode"].Value;
            return result;
        }

        if (!resetRequest.User.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["UserInactive"].Value;
            return result;
        }

        if (BCrypt.Net.BCrypt.Verify(resource.NewPassword, resetRequest.User.PasswordHash))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["NewPasswordSameAsOld"].Value;
            return result;
        }

        resetRequest.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resource.NewPassword);
        resetRequest.User.UpdatedAtUtc = now;

        resetRequest.IsUsed = true;
        resetRequest.UsedAtUtc = now;

        await RevokeActiveRefreshTokensAsync(
            resetRequest.UserId,
            localizer["PasswordResetRevokedSessions"].Value,
            cancellationToken);

        context.PasswordResetRequests.Update(resetRequest);
        context.Users.Update(resetRequest.User);

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["PasswordResetSuccessful"].Value;

        return result;
    }

    public async Task<ApiOperationResultDto<object>> ChangePasswordAsync(
    int userId,
    string? currentRefreshToken,
    ChangePasswordRequestDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        if (resource.NewPassword != resource.ConfirmPassword)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["PasswordsDoNotMatch"].Value;
            return result;
        }

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["UserInactive"].Value;
            return result;
        }

        if (!BCrypt.Net.BCrypt.Verify(resource.CurrentPassword, user.PasswordHash))
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["CurrentPasswordInvalid"].Value;
            return result;
        }

        if (BCrypt.Net.BCrypt.Verify(resource.NewPassword, user.PasswordHash))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["NewPasswordSameAsOld"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resource.NewPassword);
        user.UpdatedAtUtc = now;

        await RevokeActiveRefreshTokensAsync(
            userId,
            localizer["PasswordChangeRevokedSessions"].Value,
            cancellationToken,
            currentRefreshToken);

        context.Users.Update(user);

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["PasswordChangedSuccessfully"].Value;

        return result;
    }

    public async Task<ApiOperationResultDto<List<SessionResponseDto>>> GetMySessionsAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<SessionResponseDto>>();

        var now = DateTime.UtcNow;

        var sessions = await context.RefreshTokens
            .Where(x => x.UserId == userId &&
                        !x.IsRevoked &&
                        x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SessionResponseDto(
                x.Id,
                x.CreatedAtUtc,
                x.ExpiresAtUtc))
            .ToListAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["SessionsRetrievedSuccessfully"].Value;
        result.Result = sessions;

        return result;
    }

    public async Task<ApiOperationResultDto<object>> RevokeSessionAsync(
    int userId,
    int refreshTokenId,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        var now = DateTime.UtcNow;

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Id == refreshTokenId &&
                     x.UserId == userId &&
                     !x.IsRevoked &&
                     x.ExpiresAtUtc > now,
                cancellationToken);

        if (refreshToken is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["SessionNotFound"].Value;
            return result;
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAtUtc = now;
        refreshToken.RevokedReason = localizer["SessionRevokedByUser"].Value;

        context.RefreshTokens.Update(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["SessionRevokedSuccessfully"].Value;

        return result;
    }

    public async Task<ApiOperationResultDto<object>> RevokeAllSessionsAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        await RevokeActiveRefreshTokensAsync(
            userId,
            localizer["AllSessionsRevokedByUser"].Value,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["AllSessionsRevokedSuccessfully"].Value;

        return result;
    }

    public async Task<ApiOperationResultDto<VerifyOtpResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<VerifyOtpResponseDto>();

        var currentToken = await context.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == resource.RefreshToken, cancellationToken);

        if (currentToken is null ||
            currentToken.IsRevoked ||
            currentToken.ExpiresAtUtc <= DateTime.UtcNow ||
            !currentToken.User.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["InvalidRefreshToken"].Value;
            return result;
        }

        currentToken.IsRevoked = true;

        var accessToken = GenerateAccessToken(currentToken.User);
        var newRefreshToken = await CreateRefreshTokenAsync(currentToken.UserId, cancellationToken);

        context.RefreshTokens.Update(currentToken);
        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["TokenRefreshedSuccessfully"].Value;
        result.Result = new VerifyOtpResponseDto(accessToken, newRefreshToken.Token);

        return result;
    }

    public async Task<ApiOperationResultDto<object>> LogoutAsync(LogoutRequestDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == resource.RefreshToken, cancellationToken);

        if (refreshToken is null || refreshToken.IsRevoked)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InvalidRefreshToken"].Value;
            return result;
        }

        refreshToken.IsRevoked = true;

        context.RefreshTokens.Update(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["LogoutSuccessful"].Value;

        return result;
    }

    private async Task RevokeActiveRefreshTokensAsync(
    int userId,
    string revokedReason,
    CancellationToken cancellationToken,
    string? excludedRefreshToken = null)
    {
        var now = DateTime.UtcNow;

        var activeRefreshTokens = await context.RefreshTokens
            .Where(x => x.UserId == userId &&
                        !x.IsRevoked &&
                        x.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeRefreshTokens)
        {
            if (!string.IsNullOrWhiteSpace(excludedRefreshToken) &&
                token.Token == excludedRefreshToken)
            {
                continue;
            }

            token.IsRevoked = true;
            token.RevokedAtUtc = now;
            token.RevokedReason = revokedReason;
        }

        context.RefreshTokens.UpdateRange(activeRefreshTokens);
    }

    private static string GenerateOtpCode()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);

        return number.ToString("D6");
    }

    private string GenerateAccessToken(User user)
    {
        var issuer = configuration["Jwt:Issuer"]!;
        var audience = configuration["Jwt:Audience"]!;
        var secretKey = configuration["Jwt:SecretKey"]!;
        var expirationMinutes = configuration.GetValue<int>("Jwt:TokenExpirationMinutes");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("UserId", user.Id.ToString()),
            new("Email", user.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId, CancellationToken cancellationToken)
    {
        var expirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString(),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        return refreshToken;
    }
}