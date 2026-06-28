using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.DTOs.Auth;
using System.Security.Claims;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUnitOfWork unitOfWork,
    IStringLocalizer<AuthController> stringLocalizer
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost(nameof(Login))]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.LoginAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost(nameof(ForgotPassword))]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordRequestDto resource,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.ForgotPasswordAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [AllowAnonymous]
    [HttpPost(nameof(ResetPassword))]
    public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordRequestDto resource,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.ResetPasswordAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost(nameof(ChangePassword))]
    public async Task<IActionResult> ChangePassword(
    [FromBody] ChangePasswordRequestDto resource,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var userId = GetUserIdFromClaims();

        if (userId is null)
        {
            return Unauthorized(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized.ToString(),
                Message = stringLocalizer["Unauthorized"].Value
            });
        }

        var currentToken = GetBearerToken();

        var result = await unitOfWork.Auth.ChangePasswordAsync(
            userId.Value,
            currentToken,
            resource,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpGet(nameof(MySessions))]
    public async Task<IActionResult> MySessions(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        if (userId is null)
        {
            return Unauthorized(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized.ToString(),
                Message = stringLocalizer["Unauthorized"].Value
            });
        }

        var result = await unitOfWork.Auth.GetMySessionsAsync(
            userId.Value,
            cancellationToken);

        return Ok(result);
    }

    [Authorize]
    [HttpPost(nameof(RevokeSession) + "/{refreshTokenId:int}")]
    public async Task<IActionResult> RevokeSession(
    int refreshTokenId,
    CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        if (userId is null)
        {
            return Unauthorized(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized.ToString(),
                Message = stringLocalizer["Unauthorized"].Value
            });
        }

        var result = await unitOfWork.Auth.RevokeSessionAsync(
            userId.Value,
            refreshTokenId,
            cancellationToken);

        return result.Success ? Ok(result) : NotFound(result);
    }

    [Authorize]
    [HttpPost(nameof(RevokeAllSessions))]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();

        if (userId is null)
        {
            return Unauthorized(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized.ToString(),
                Message = stringLocalizer["Unauthorized"].Value
            });
        }

        var result = await unitOfWork.Auth.RevokeAllSessionsAsync(
            userId.Value,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost(nameof(VerifyOtp))]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.VerifyOtpAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost(nameof(RefreshToken))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.RefreshTokenAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost(nameof(Logout))]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Auth.LogoutAsync(resource, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private int? GetUserIdFromClaims()
    {
        var userIdValue = User.FindFirstValue("UserId");

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    private string? GetBearerToken()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorizationHeader["Bearer ".Length..].Trim();
    }

}