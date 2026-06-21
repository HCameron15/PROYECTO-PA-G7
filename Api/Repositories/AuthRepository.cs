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