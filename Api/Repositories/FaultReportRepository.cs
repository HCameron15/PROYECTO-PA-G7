using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Constants;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReports;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class FaultReportRepository(
    AppDbContext context,
    IStringLocalizer<FaultReportRepository> localizer,
    IEmailNotificationService emailNotificationService)
    : Repository<FaultReport>(context), IFaultReportRepository
{
    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetAllFaultReportsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var reports = await GetFaultReportsQuery()
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        result.Success = reports.Count > 0;
        result.Code = result.Success ? "200" : "404";
        result.Message = result.Success
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = result.Success
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
        result.Code = result.Success ? "200" : "404";
        result.Message = result.Success
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportNotFound"].Value;
        result.Result = report is null ? null : MapToDto(report);

        return result;
    }

    public async Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<FaultReportDto>>();

        var normalizedStatus = FaultReportStatuses.All
            .FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalizedStatus is null)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportStatus"].Value;
            return result;
        }

        var reports = await GetFaultReportsQuery()
            .Where(x => x.Status == normalizedStatus)
            .OrderByDescending(x => x.ReportedAtUtc)
            .ToListAsync(cancellationToken);

        result.Success = reports.Count > 0;
        result.Code = result.Success ? "200" : "404";
        result.Message = result.Success
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = result.Success
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

        result.Success = reports.Count > 0;
        result.Code = result.Success ? "200" : "404";
        result.Message = result.Success
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = result.Success
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

        result.Success = reports.Count > 0;
        result.Code = result.Success ? "200" : "404";
        result.Message = result.Success
            ? localizer["OperationSuccessful"].Value
            : localizer["FaultReportsNotFound"].Value;
        result.Result = result.Success
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
            .FirstOrDefaultAsync(x => x.Id == reportedByUserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            result.Success = false;
            result.Code = "401";
            result.Message = localizer["UserNotAvailable"].Value;
            return result;
        }

        if (!user.Role.Name.Equals(RoleNames.Instructor, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "403";
            result.Message = localizer["InstructorRoleRequired"].Value;
            return result;
        }

        var priority = FaultReportPriorities.All
            .FirstOrDefault(x => x.Equals(resource.Priority.Trim(), StringComparison.OrdinalIgnoreCase));

        if (priority is null)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportPriority"].Value;
            return result;
        }

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .FirstOrDefaultAsync(x => x.Id == resource.EquipmentId, cancellationToken);

        if (equipment is null || !equipment.IsActive)
        {
            result.Success = false;
            result.Code = "404";
            result.Message = localizer["EquipmentNotFound"].Value;
            return result;
        }

        if (!equipment.Status.Equals(EquipmentStatuses.Operational, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
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
            UpdatedAtUtc = now
        };

        equipment.Status = EquipmentStatuses.UnderRepair;
        equipment.UpdatedAtUtc = now;

        await Context.FaultReports.AddAsync(faultReport, cancellationToken);
        Context.Equipment.Update(equipment);

        await Context.SaveChangesAsync(cancellationToken);

        var created = await GetFaultReportsQuery()
            .FirstAsync(x => x.Id == faultReport.Id, cancellationToken);

        await emailNotificationService.SendReportCreatedAsync(
            created,
            cancellationToken);

        result.Success = true;
        result.Code = "201";
        result.Message = localizer["FaultReportCreatedSuccessfully"].Value;
        result.Result = MapToDto(created);

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
            .Include(x => x.AssignedTechnician)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = "404";
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(FaultReportStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["ClosedFaultReportCannotBeModified"].Value;
            return result;
        }

        var priority = FaultReportPriorities.All
            .FirstOrDefault(x => x.Equals(resource.Priority.Trim(), StringComparison.OrdinalIgnoreCase));

        if (priority is null)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportPriority"].Value;
            return result;
        }

        var status = FaultReportStatuses.All
            .FirstOrDefault(x => x.Equals(resource.Status.Trim(), StringComparison.OrdinalIgnoreCase));

        if (status is null)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportStatus"].Value;
            return result;
        }

        if (status == FaultReportStatuses.Closed)
        {
            result.Success = false;
            result.Code = "400";
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
        result.Code = "200";
        result.Message = localizer["FaultReportUpdatedSuccessfully"].Value;
        result.Result = MapToDto(faultReport);

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> AssignFaultReportAsync(
        int id,
        int technicianUserId,
        CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var technician = await Context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == technicianUserId, cancellationToken);

        if (technician is null || !technician.IsActive)
        {
            result.Success = false;
            result.Code = "401";
            result.Message = localizer["UserNotAvailable"].Value;
            return result;
        }

        if (!technician.Role.Name.Equals(RoleNames.Technician, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "403";
            result.Message = localizer["TechnicianRoleRequired"].Value;
            return result;
        }

        var faultReport = await Context.FaultReports
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Laboratory)
            .Include(x => x.ReportedByUser)
            .Include(x => x.AssignedTechnician)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = "404";
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(FaultReportStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["ClosedFaultReportCannotBeModified"].Value;
            return result;
        }

        if (!faultReport.Status.Equals(FaultReportStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportStatusTransition"].Value;
            return result;
        }

        var now = DateTime.UtcNow;
        var previousStatus = faultReport.Status;

        faultReport.AssignedTechnicianId = technician.Id;
        faultReport.AssignedTechnician = technician;
        faultReport.Status = FaultReportStatuses.InProgress;
        faultReport.UpdatedAtUtc = now;

        var log = new FaultReportStatusLog
        {
            FaultReportId = faultReport.Id,
            ChangedByUserId = technician.Id,
            PreviousStatus = previousStatus,
            NewStatus = FaultReportStatuses.InProgress,
            Notes = null,
            ChangedAtUtc = now
        };

        await Context.FaultReportStatusLogs.AddAsync(log, cancellationToken);
        Context.FaultReports.Update(faultReport);

        await Context.SaveChangesAsync(cancellationToken);

        await emailNotificationService.SendReportAssignedAsync(
            faultReport,
            technician,
            cancellationToken);

        result.Success = true;
        result.Code = "200";
        result.Message = localizer["FaultReportAssignedSuccessfully"].Value;
        result.Result = MapToDto(faultReport);

        return result;
    }

    public async Task<ApiOperationResultDto<FaultReportDto>> UpdateFaultReportStatusAsync(
        int id,
        int changedByUserId,
        UpdateFaultReportStatusDto resource,
        CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<FaultReportDto>();

        var user = await Context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == changedByUserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            result.Success = false;
            result.Code = "401";
            result.Message = localizer["UserNotAvailable"].Value;
            return result;
        }

        var faultReport = await Context.FaultReports
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Laboratory)
            .Include(x => x.ReportedByUser)
            .Include(x => x.AssignedTechnician)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = "404";
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(FaultReportStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["ClosedFaultReportCannotBeModified"].Value;
            return result;
        }

        if (faultReport.AssignedTechnicianId != changedByUserId)
        {
            result.Success = false;
            result.Code = "403";
            result.Message = localizer["OnlyAssignedTechnicianCanUpdateStatus"].Value;
            return result;
        }

        var newStatus = FaultReportStatuses.All
            .FirstOrDefault(x => x.Equals(resource.NewStatus.Trim(), StringComparison.OrdinalIgnoreCase));

        if (newStatus is null)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportStatus"].Value;
            return result;
        }

        var isValidTransition =
            faultReport.Status == FaultReportStatuses.InProgress &&
            newStatus == FaultReportStatuses.Resolved;

        if (!isValidTransition)
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["InvalidFaultReportStatusTransition"].Value;
            return result;
        }

        var now = DateTime.UtcNow;
        var previousStatus = faultReport.Status;

        faultReport.Status = newStatus;
        faultReport.UpdatedAtUtc = now;

        var log = new FaultReportStatusLog
        {
            FaultReportId = faultReport.Id,
            ChangedByUserId = changedByUserId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Notes = string.IsNullOrWhiteSpace(resource.Notes)
                ? null
                : resource.Notes.Trim(),
            ChangedAtUtc = now
        };

        await Context.FaultReportStatusLogs.AddAsync(log, cancellationToken);
        Context.FaultReports.Update(faultReport);

        await Context.SaveChangesAsync(cancellationToken);

        if (newStatus == FaultReportStatuses.Resolved)
        {
            await emailNotificationService.SendStatusChangedAsync(
                faultReport,
                newStatus,
                cancellationToken);
        }

        result.Success = true;
        result.Code = "200";
        result.Message = localizer["FaultReportStatusUpdatedSuccessfully"].Value;
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
            .Include(x => x.AssignedTechnician)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faultReport is null)
        {
            result.Success = false;
            result.Code = "404";
            result.Message = localizer["FaultReportNotFound"].Value;
            return result;
        }

        if (faultReport.Status.Equals(FaultReportStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
            result.Message = localizer["FaultReportAlreadyClosed"].Value;
            return result;
        }

        if (!faultReport.Status.Equals(FaultReportStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Code = "400";
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

        await emailNotificationService.SendReportClosedAsync(
            faultReport,
            cancellationToken);

        result.Success = true;
        result.Code = "200";
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
            .Include(x => x.AssignedTechnician)
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
            report.AssignedTechnicianId,
            report.AssignedTechnician is null
                ? string.Empty
                : $"{report.AssignedTechnician.FirstName} {report.AssignedTechnician.LastName}".Trim(),
            report.AssignedTechnician?.Email ?? string.Empty,
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
