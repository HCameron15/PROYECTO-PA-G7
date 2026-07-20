using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Constants;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.Dashboard;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class DashboardRepository(
    AppDbContext context,
    IStringLocalizer<DashboardRepository> localizer)
    : IDashboardRepository
{
    public async Task<ApiOperationResultDto<GeneralSummaryDto>> GetGeneralSummaryAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var denied = await EnsureAdministratorAsync<GeneralSummaryDto>(
            userId,
            cancellationToken);

        if (denied is not null)
        {
            return denied;
        }

        var reportCounts = await context.FaultReports
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalReports = group.Count(),
                PendingCount = group.Count(x => x.Status == FaultReportStatuses.Pending),
                InProgressCount = group.Count(x => x.Status == FaultReportStatuses.InProgress),
                ResolvedCount = group.Count(x => x.Status == FaultReportStatuses.Resolved),
                ClosedCount = group.Count(x => x.Status == FaultReportStatuses.Closed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var equipmentCounts = await context.Equipment
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalEquipment = group.Count(),
                EquipmentUnderRepair = group.Count(x =>
                    x.Status == EquipmentStatuses.UnderRepair)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var summary = new GeneralSummaryDto(
            reportCounts?.TotalReports ?? 0,
            reportCounts?.PendingCount ?? 0,
            reportCounts?.InProgressCount ?? 0,
            reportCounts?.ResolvedCount ?? 0,
            reportCounts?.ClosedCount ?? 0,
            equipmentCounts?.TotalEquipment ?? 0,
            equipmentCounts?.EquipmentUnderRepair ?? 0);

        return Success(summary);
    }

    public async Task<ApiOperationResultDto<List<ReportsByLabDto>>> GetReportsByLabAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var denied = await EnsureAdministratorAsync<List<ReportsByLabDto>>(
            userId,
            cancellationToken);

        if (denied is not null)
        {
            return denied;
        }

        var reports = await context.FaultReports
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.Equipment.LaboratoryId,
                x.Equipment.Laboratory!.Name
            })
            .Select(group => new ReportsByLabDto(
                group.Key.LaboratoryId,
                group.Key.Name,
                group.Count(),
                group.Count(x => x.Status == FaultReportStatuses.Pending),
                group.Count(x => x.Status == FaultReportStatuses.InProgress)))
            .OrderByDescending(x => x.TotalReports)
            .ThenBy(x => x.LabName)
            .ToListAsync(cancellationToken);

        return Success(reports);
    }

    public async Task<ApiOperationResultDto<List<ReportsByStatusDto>>> GetReportsByStatusAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var denied = await EnsureAdministratorAsync<List<ReportsByStatusDto>>(
            userId,
            cancellationToken);

        if (denied is not null)
        {
            return denied;
        }

        var reports = await context.FaultReports
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(group => new ReportsByStatusDto(
                group.Key,
                group.Count()))
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);

        return Success(reports);
    }

    public async Task<ApiOperationResultDto<List<ReportsByTechnicianDto>>> GetReportsByTechnicianAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var denied = await EnsureAdministratorAsync<List<ReportsByTechnicianDto>>(
            userId,
            cancellationToken);

        if (denied is not null)
        {
            return denied;
        }

        var technicians = await context.Users
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Role.IsActive &&
                x.Role.Name == RoleNames.Technician)
            .Select(x => new ReportsByTechnicianDto(
                x.Id,
                (x.FirstName + " " + x.LastName).Trim(),
                x.AssignedFaultReports.Count(),
                x.AssignedFaultReports.Count(report =>
                    report.Status == FaultReportStatuses.Resolved)))
            .OrderByDescending(x => x.AssignedCount)
            .ThenBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return Success(technicians);
    }

    public async Task<ApiOperationResultDto<AverageResolutionTimeDto>> GetAverageResolutionTimeAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var denied = await EnsureAdministratorAsync<AverageResolutionTimeDto>(
            userId,
            cancellationToken);

        if (denied is not null)
        {
            return denied;
        }

        var resolutionSeconds = context.FaultReportStatusLogs
            .AsNoTracking()
            .Where(x => x.NewStatus == FaultReportStatuses.Resolved)
            .GroupBy(x => new
            {
                x.FaultReportId,
                x.FaultReport.ReportedAtUtc
            })
            .Select(group => EF.Functions.DateDiffSecond(
                group.Key.ReportedAtUtc,
                group.Min(x => x.ChangedAtUtc)));

        var statistics = await resolutionSeconds
            .GroupBy(_ => 1)
            .Select(group => new AverageResolutionTimeDto(
                group.Average(x => (double)x) / 3600d,
                group.Min() / 3600d,
                group.Max() / 3600d))
            .FirstOrDefaultAsync(cancellationToken)
            ?? new AverageResolutionTimeDto(0, 0, 0);

        return Success(statistics);
    }

    private async Task<ApiOperationResultDto<T>?> EnsureAdministratorAsync<T>(
        int userId,
        CancellationToken cancellationToken)
    {
        var isAdministrator = await context.Users
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == userId &&
                x.IsActive &&
                x.Role.IsActive &&
                x.Role.Name == RoleNames.Admin,
                cancellationToken);

        if (isAdministrator)
        {
            return null;
        }

        return new ApiOperationResultDto<T>
        {
            Success = false,
            Code = StatusCodes.Status403Forbidden.ToString(),
            Message = localizer["DashboardAccessDenied"].Value
        };
    }

    private ApiOperationResultDto<T> Success<T>(T value)
    {
        return new ApiOperationResultDto<T>
        {
            Success = true,
            Code = StatusCodes.Status200OK.ToString(),
            Message = localizer["DashboardDataRetrieved"].Value,
            Result = value
        };
    }
}
