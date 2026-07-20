using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController(
    IUnitOfWork unitOfWork,
    IStringLocalizer<DashboardController> localizer)
    : ControllerBase
{
    [HttpGet(nameof(GetGeneralSummary))]
    public Task<IActionResult> GetGeneralSummary(CancellationToken cancellationToken) =>
        ExecuteAsync((userId, token) =>
            unitOfWork.Dashboard.GetGeneralSummaryAsync(userId, token),
            cancellationToken);

    [HttpGet(nameof(GetReportsByLab))]
    public Task<IActionResult> GetReportsByLab(CancellationToken cancellationToken) =>
        ExecuteAsync((userId, token) =>
            unitOfWork.Dashboard.GetReportsByLabAsync(userId, token),
            cancellationToken);

    [HttpGet(nameof(GetReportsByStatus))]
    public Task<IActionResult> GetReportsByStatus(CancellationToken cancellationToken) =>
        ExecuteAsync((userId, token) =>
            unitOfWork.Dashboard.GetReportsByStatusAsync(userId, token),
            cancellationToken);

    [HttpGet(nameof(GetReportsByTechnician))]
    public Task<IActionResult> GetReportsByTechnician(CancellationToken cancellationToken) =>
        ExecuteAsync((userId, token) =>
            unitOfWork.Dashboard.GetReportsByTechnicianAsync(userId, token),
            cancellationToken);

    [HttpGet(nameof(GetAverageResolutionTime))]
    public Task<IActionResult> GetAverageResolutionTime(CancellationToken cancellationToken) =>
        ExecuteAsync((userId, token) =>
            unitOfWork.Dashboard.GetAverageResolutionTimeAsync(userId, token),
            cancellationToken);

    private async Task<IActionResult> ExecuteAsync<T>(
        Func<int, CancellationToken, Task<ApiOperationResultDto<T>>> operation,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue("UserId");

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized.ToString(),
                Message = localizer["AuthenticatedUserNotFound"].Value
            });
        }

        var result = await operation(userId, cancellationToken);

        return result.Success
            ? Ok(result)
            : StatusCode(StatusCodes.Status403Forbidden, result);
    }
}
