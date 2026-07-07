namespace Uam.AdvancedProgramming.Api.DTOs.FaultReports;

public record FaultReportDto(
    int Id,
    int EquipmentId,
    string EquipmentCode,
    int LaboratoryId,
    string LaboratoryName,
    int ReportedByUserId,
    string ReportedByName,
    string ReportedByEmail,
    int? AssignedTechnicianId,
    string AssignedTechnicianName,
    string AssignedTechnicianEmail,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTime ReportedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);