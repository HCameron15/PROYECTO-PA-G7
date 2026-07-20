namespace Uam.AdvancedProgramming.Api.DTOs.Dashboard;

public record ReportsByTechnicianDto(
    int TechnicianId,
    string FullName,
    int AssignedCount,
    int ResolvedCount);
