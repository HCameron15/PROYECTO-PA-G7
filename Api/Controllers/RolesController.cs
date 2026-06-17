using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RolesController(IUnitOfWork unitOfWork, IStringLocalizer<RolesController> stringLocalizer) : ControllerBase
{
    [HttpGet(nameof(GetAllRoles))]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Roles.GetAllRolesAsync(cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetRoleById) + "/{id:int}")]
    public async Task<IActionResult> GetRoleById(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Roles.GetRoleByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost(nameof(CreateRole))]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Roles.CreateRoleAsync(resource, cancellationToken);
        return result.Success ? Created(string.Empty, result) : BadRequest(result);
    }

    [HttpPut(nameof(UpdateRole) + "/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Roles.UpdateRoleAsync(id, resource, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString() ? NotFound(result) : BadRequest(result);
    }

    [HttpDelete(nameof(DeleteRole) + "/{id:int}")]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Roles.DeleteRoleAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}