using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EquipmentController(IUnitOfWork unitOfWork, IStringLocalizer<EquipmentController> stringLocalizer) : ControllerBase
{
    [HttpGet(nameof(GetAllEquipment))]
    public async Task<IActionResult> GetAllEquipment(CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Equipment.GetAllEquipmentAsync(cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetEquipmentById) + "/{id:int}")]
    public async Task<IActionResult> GetEquipmentById(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Equipment.GetEquipmentByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(nameof(GetEquipmentByLaboratory) + "/{laboratoryId:int}")]
    public async Task<IActionResult> GetEquipmentByLaboratory(int laboratoryId, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Equipment.GetEquipmentByLaboratoryAsync(laboratoryId, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost(nameof(CreateEquipment))]
    public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Equipment.CreateEquipmentAsync(resource, cancellationToken);
        return result.Success ? Created(string.Empty, result) : BadRequest(result);
    }

    [HttpPut(nameof(UpdateEquipment) + "/{id:int}")]
    public async Task<IActionResult> UpdateEquipment(int id, [FromBody] UpdateEquipmentDto resource, CancellationToken cancellationToken)
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

        var result = await unitOfWork.Equipment.UpdateEquipmentAsync(id, resource, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString() ? NotFound(result) : BadRequest(result);
    }

    [HttpDelete(nameof(DeleteEquipment) + "/{id:int}")]
    public async Task<IActionResult> DeleteEquipment(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Equipment.DeleteEquipmentAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}