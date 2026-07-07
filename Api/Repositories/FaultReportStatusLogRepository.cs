using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReportStatusLogs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class FaultReportStatusLogRepository(
    AppDbContext context,
    IStringLocalizer<FaultReportStatusLogRepository> localizer)
    : Repository<FaultReportStatusLog>(context), IFaultReportStatusLogRepository
{
    public async Task<ApiOperationResultDto<List<FaultReportStatusLogDto>>> GetLogsByFaultReportAsync(
        int faultReportId,
        CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportStatusLogDto>>();

        var logs = await Context.FaultReportStatusLogs
            .Include(x => x.ChangedByUser)
            .Where(x => x.FaultReportId == faultReportId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Select(x => new FaultReportStatusLogDto(
                x.Id,
                x.FaultReportId,
                x.ChangedByUserId,
                $"{x.ChangedByUser.FirstName} {x.ChangedByUser.LastName}",
                x.PreviousStatus,
                x.NewStatus,
                x.Notes,
                x.ChangedAtUtc
            ))
            .ToListAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["OperationSuccessful"].Value;
        result.Result = logs;

        return result;
    }
}