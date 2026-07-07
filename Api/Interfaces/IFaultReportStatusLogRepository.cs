using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReportStatusLogs;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IFaultReportStatusLogRepository
{
    Task<ApiOperationResultDto<List<FaultReportStatusLogDto>>> GetLogsByFaultReportAsync(
        int faultReportId,
        CancellationToken cancellationToken = default);
}