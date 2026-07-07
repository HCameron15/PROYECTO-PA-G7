using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FaultReportStatusLogsController(
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(nameof(GetLogsByFaultReport) + "/{faultReportId:int}")]
    public async Task<IActionResult> GetLogsByFaultReport(
        int faultReportId,
        CancellationToken cancellationToken)
    {
        var result = await unitOfWork.FaultReportStatusLogs
            .GetLogsByFaultReportAsync(
                faultReportId,
                cancellationToken);

        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code switch
        {
            "404" => NotFound(result),
            _ => BadRequest(result)
        };
    }
}