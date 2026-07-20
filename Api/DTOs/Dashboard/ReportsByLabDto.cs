namespace Uam.AdvancedProgramming.Api.DTOs.Dashboard;

public record ReportsByLabDto(
    int LabId,
    string LabName,
    int TotalReports,
    int PendingCount,
    int InProgressCount);
