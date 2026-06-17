using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LaboratoriesController(IUnitOfWork unitOfWork, IStringLocalizer<LaboratoriesController> stringLocalizer) : ControllerBase
{
    [HttpGet(nameof(GetAllLaboratories))]
    public async Task<IActionResult> GetAllLaboratories(CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Laboratories.GetAllLaboratoriesAsync(cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetLaboratoryById) + "/{id:int}")]
    public async Task<IActionResult> GetLaboratoryById(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Laboratories.GetLaboratoryByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost(nameof(CreateLaboratory))]
    public async Task<IActionResult> CreateLaboratory([FromBody] CreateLaboratoryDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Laboratories.CreateLaboratoryAsync(resource, cancellationToken);
        return result.Success ? Created(string.Empty, result) : BadRequest(result);
    }

    [HttpPut(nameof(UpdateLaboratory) + "/{id:int}")]
    public async Task<IActionResult> UpdateLaboratory(int id, [FromBody] UpdateLaboratoryDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Laboratories.UpdateLaboratoryAsync(id, resource, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString() ? NotFound(result) : BadRequest(result);
    }

    [HttpDelete(nameof(DeleteLaboratory) + "/{id:int}")]
    public async Task<IActionResult> DeleteLaboratory(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Laboratories.DeleteLaboratoryAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}