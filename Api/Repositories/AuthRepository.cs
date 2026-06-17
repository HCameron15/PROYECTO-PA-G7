using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class AuthRepository(
    AppDbContext context,
    IConfiguration configuration,
    IStringLocalizer<AuthRepository> localizer
) : IAuthRepository
{
    public async Task<ApiOperationResultDto<LoginResponseDto>> LoginAsync(LoginRequestDto resource, CancellationToken cancellationToken = default)
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

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["LoginSuccessful"].Value;
        result.Result = new LoginResponseDto(
            accessToken,
            refreshToken.Token,
            "Bearer",
            configuration.GetValue<int>("Jwt:TokenExpirationMinutes") * 60
        );

        return result;
    }

    public async Task<ApiOperationResultDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<LoginResponseDto>();

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
        result.Result = new LoginResponseDto(
            accessToken,
            newRefreshToken.Token,
            "Bearer",
            configuration.GetValue<int>("Jwt:TokenExpirationMinutes") * 60
        );

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