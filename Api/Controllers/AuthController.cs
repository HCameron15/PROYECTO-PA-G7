using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

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
}