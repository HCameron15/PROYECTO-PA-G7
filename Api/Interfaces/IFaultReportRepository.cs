using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.DTOs.FaultReports;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IFaultReportRepository
{
    Task<ApiOperationResultDto<List<FaultReportDto>>> GetAllFaultReportsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> GetFaultReportByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByStatusAsync(
        string status,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByEquipmentAsync(
        int equipmentId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<FaultReportDto>>> GetFaultReportsByUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> CreateFaultReportAsync(
        int reportedByUserId,
        CreateFaultReportDto resource,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> UpdateFaultReportAsync(
        int id,
        UpdateFaultReportDto resource,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> CloseFaultReportAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> AssignFaultReportAsync(
        int id,
        int technicianUserId,
        CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<FaultReportDto>> UpdateFaultReportStatusAsync(
        int id,
        int changedByUserId,
        UpdateFaultReportStatusDto resource,
        CancellationToken cancellationToken = default);
}