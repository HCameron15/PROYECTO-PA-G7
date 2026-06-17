using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IUnitOfWork unitOfWork, IStringLocalizer<UsersController> stringLocalizer) : ControllerBase
{
    [HttpGet(nameof(GetAllUsers))]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Users.GetAllUsersAsync(cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetUserById) + "/{id:int}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Users.GetUserByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetUsersByRole) + "/{roleId:int}")]
    public async Task<IActionResult> GetUsersByRole(int roleId, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Users.GetUsersByRoleAsync(roleId, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost(nameof(CreateUser))]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Users.CreateUserAsync(resource, cancellationToken);
        return result.Success ? Created(string.Empty, result) : BadRequest(result);
    }

    [HttpPut(nameof(UpdateUser) + "/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Users.UpdateUserAsync(id, resource, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString() ? NotFound(result) : BadRequest(result);
    }

    [HttpDelete(nameof(DeleteUser) + "/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Users.DeleteUserAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}