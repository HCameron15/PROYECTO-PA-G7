using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReports;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;
using Microsoft.EntityFrameworkCore;
using Uam.AdvancedProgramming.Api.Constants;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class FaultReportRepository(
    AppDbContext context,
    IStringLocalizer<FaultReportRepository> localizer)
    : Repository<FaultReport>(context), IFaultReportRepository
{
    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetAllFaultReportsAsync(
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var reports = await GetFaultReportsQuery()
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        var hasRecords = reports.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords
            ? StatusCodes.Status200OK.ToString()
            : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = hasRecords
            ? reports.Select(MapToDto).ToList()
            : null;

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> GetFaultReportByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var report = await GetFaultReportsQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        result.Success = report is not null;
        result.Code = report is not null
            ? StatusCodes.Status200OK.ToString()
            : StatusCodes.Status404NotFound.ToString();
        result.Message = report is not null
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportNotFound"].Value;
        result.Result = report is null
            ? null
            : MapToDto(report);

        return result;
    }

    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByStatusAsync(
    string status,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var normalizedStatus = FaultReportStatuses.All
            .FirstOrDefault(x =>
                x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalizedStatus is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InvalidFaultReportStatus"].Value;
            return result;
        }

        var reports = await GetFaultReportsQuery()
            .Where(x => x.Status == normalizedStatus)
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        var hasRecords = reports.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords
            ? StatusCodes.Status200OK.ToString()
            : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = hasRecords
            ? reports.Select(MapToDto).ToList()
            : null;

        return result;
    }

    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByEquipmentAsync(
    int equipmentId,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var reports = await GetFaultReportsQuery()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        var hasRecords = reports.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords
            ? StatusCodes.Status200OK.ToString()
            : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = hasRecords
            ? reports.Select(MapToDto).ToList()
            : null;

        return result;
    }

    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByUserAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var reports = await GetFaultReportsQuery()
            .Where(x => x.ReportedByUserId == userId)
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        var hasRecords = reports.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords
            ? StatusCodes.Status200OK.ToString()
            : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = hasRecords
            ? reports.Select(MapToDto).ToList()
            : null;

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> CreateFaultReportAsync(
    int reportedByUserId,
    CreateFaultReportDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var user = await Context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Id == reportedByUserId,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status401Unauthorized.ToString();
            result.Message = localizer["UserNotAvailable"].Value;
            return result;
        }

        if (!user.Role.Name.Equals(
                RoleNames.Instructor,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = StatusCodes.Status403Forbidden.ToString();
            result.Message = localizer["InstructorRoleRequired"].Value;
            return result;
        }

        var priority = FaultReportPriorities.All
            .FirstOrDefault(x =>
                x.Equals(
                    resource.Priority.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (priority is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InvalidFaultReportPriority"].Value;
            return result;
        }

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .FirstOrDefaultAsync(
                x => x.Id == resource.EquipmentId,
                cancellationToken);

        if (equipment is null || !equipment.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["EquipmentNotFound"].Value;
            return result;
        }

        if (!equipment.Status.Equals(
                EquipmentStatuses.Operational,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["EquipmentNotOperational"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        var faultReport = new FaultReport
        {
            EquipmentId = equipment.Id,
            ReportedByUserId = user.Id,
            Title = resource.Title.Trim(),
            Description = resource.Description.Trim(),
            Priority = priority,
            Status = FaultReportStatuses.Pending,
            ReportedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Equipment = equipment,
            ReportedByUser = user
        };

        equipment.Status = EquipmentStatuses.UnderRepair;
        equipment.UpdatedAtUtc = now;

        await Context.FaultReports.AddAsync(
            faultReport,
            cancellationToken);

        Context.Equipment.Update(equipment);

        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status201Created.ToString();
        result.Message = localizer["FaultReportCreatedSuccessfully"].Value;
        result.Result = MapToDto(faultReport);

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> UpdateFaultReportAsync(
    int id,
    UpdateFaultReportDto resource,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var faultReport = await Context.FaultReports
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Laboratory)
            .Include(x => x.ReportedByUser)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(
                FaultReportStatuses.Closed,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["ClosedFaultReportCannotBeModified"].Value;
            return result;
        }

        var priority = FaultReportPriorities.All
            .FirstOrDefault(x =>
                x.Equals(
                    resource.Priority.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (priority is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InvalidFaultReportPriority"].Value;
            return result;
        }

        var status = FaultReportStatuses.All
            .FirstOrDefault(x =>
                x.Equals(
                    resource.Status.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        if (status is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InvalidFaultReportStatus"].Value;
            return result;
        }

        if (status == FaultReportStatuses.Closed)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["UseCloseFaultReportEndpoint"].Value;
            return result;
        }

        faultReport.Title = resource.Title.Trim();
        faultReport.Description = resource.Description.Trim();
        faultReport.Priority = priority;
        faultReport.Status = status;
        faultReport.UpdatedAtUtc = DateTime.UtcNow;

        Context.FaultReports.Update(faultReport);

        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["FaultReportUpdatedSuccessfully"].Value;
        result.Result = MapToDto(faultReport);

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> CloseFaultReportAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var faultReport = await Context.FaultReports
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Laboratory)
            .Include(x => x.ReportedByUser)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(
                FaultReportStatuses.Closed,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["FaultReportAlreadyClosed"].Value;
            return result;
        }

        if (!faultReport.Status.Equals(
                FaultReportStatuses.Resolved,
                StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["FaultReportMustBeResolved"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        faultReport.Status = FaultReportStatuses.Closed;
        faultReport.UpdatedAtUtc = now;

        faultReport.Equipment.Status = EquipmentStatuses.Operational;
        faultReport.Equipment.UpdatedAtUtc = now;

        Context.FaultReports.Update(faultReport);
        Context.Equipment.Update(faultReport.Equipment);

        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["FaultReportClosedSuccessfully"].Value;
        result.Result = MapToDto(faultReport);

        return result;
    }

    private IQueryable<FaultReport> GetFaultReportsQuery()
    {
        return Context.FaultReports
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Laboratory)
            .Include(x => x.ReportedByUser)
            .AsNoTracking();
    }
    private static FaultReportDto MapToDto(FaultReport report)
    {
        return new FaultReportDto(
            report.Id,
            report.EquipmentId,
            report.Equipment.Code,
            report.Equipment.LaboratoryId,
            report.Equipment.Laboratory?.Name ?? string.Empty,
            report.ReportedByUserId,
            $"{report.ReportedByUser.FirstName} {report.ReportedByUser.LastName}".Trim(),
            report.ReportedByUser.Email,
            report.Title,
            report.Description,
            report.Priority,
            report.Status,
            report.ReportedAtUtc,
            report.CreatedAtUtc,
            report.UpdatedAtUtc
        );
    }
}