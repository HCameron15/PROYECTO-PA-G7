using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.Dashboard;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IDashboardRepository
{
    Task<ApiOperationResultDto<GeneralSummaryDto>> GetGeneralSummaryAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<ReportsByLabDto>>> GetReportsByLabAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<ReportsByStatusDto>>> GetReportsByStatusAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<ReportsByTechnicianDto>>> GetReportsByTechnicianAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<AverageResolutionTimeDto>> GetAverageResolutionTimeAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
