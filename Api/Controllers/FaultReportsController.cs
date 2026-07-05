using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReports;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FaultReportsController(
    IUnitOfWork unitOfWork,
    IStringLocalizer<FaultReportsController> localizer
) : ControllerBase
{
    [HttpGet(nameof(GetAllFaultReports))]
    public async Task<IActionResult> GetAllFaultReports(
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .GetAllFaultReportsAsync(cancellationToken);

        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    [HttpGet(nameof(GetFaultReportById) + "/{id:int}")]
    public async Task<IActionResult> GetFaultReportById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .GetFaultReportByIdAsync(id, cancellationToken);

        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    [HttpGet(nameof(GetFaultReportsByStatus) + "/{status}")]
    public async Task<IActionResult> GetFaultReportsByStatus(
        string status,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .GetFaultReportsByStatusAsync(status, cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status400BadRequest.ToString()
            ? BadRequest(result)
            : NotFound(result);
    }

    [HttpGet(nameof(GetFaultReportsByEquipment) + "/{equipmentId:int}")]
    public async Task<IActionResult> GetFaultReportsByEquipment(
        int equipmentId,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .GetFaultReportsByEquipmentAsync(
                equipmentId,
                cancellationToken);

        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    [HttpGet(nameof(GetFaultReportsByUser) + "/{userId:int}")]
    public async Task<IActionResult> GetFaultReportsByUser(
        int userId,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .GetFaultReportsByUserAsync(
                userId,
                cancellationToken);

        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    [HttpPost(nameof(CreateFaultReport))]
    public async Task<IActionResult> CreateFaultReport(
        [FromBody] CreateFaultReportDto resource,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateInvalidModelResult());
        }

        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return Unauthorized(CreateUnauthorizedResult());
        }

        var result = await unitOfWork.FaultReports
            .CreateFaultReportAsync(
                userId.Value,
                resource,
                cancellationToken);

        if (result.Success)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }

        return result.Code switch
        {
            "401" => Unauthorized(result),
            "403" => StatusCode(
                StatusCodes.Status403Forbidden,
                result),
            "404" => NotFound(result),
            _ => BadRequest(result)
        };
    }

    [HttpPut(nameof(UpdateFaultReport) + "/{id:int}")]
    public async Task<IActionResult> UpdateFaultReport(
        int id,
        [FromBody] UpdateFaultReportDto resource,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateInvalidModelResult());
        }

        var result = await unitOfWork.FaultReports
            .UpdateFaultReportAsync(
                id,
                resource,
                cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString()
            ? NotFound(result)
            : BadRequest(result);
    }

    [HttpPost(nameof(CloseFaultReport) + "/{id:int}")]
    public async Task<IActionResult> CloseFaultReport(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReports
            .CloseFaultReportAsync(
                id,
                cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString()
            ? NotFound(result)
            : BadRequest(result);
    }

    private int? GetAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue("UserId");

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    private ApiOperationResultDto<object> CreateInvalidModelResult()
    {
        return new ApiOperationResultDto<object>
        {
            Success = false,
            Code = StatusCodes.Status400BadRequest.ToString(),
            Message = localizer["InvalidModel"].Value
        };
    }

    private ApiOperationResultDto<object> CreateUnauthorizedResult()
    {
        return new ApiOperationResultDto<object>
        {
            Success = false,
            Code = StatusCodes.Status401Unauthorized.ToString(),
            Message = localizer["Unauthorized"].Value
        };
    }
}