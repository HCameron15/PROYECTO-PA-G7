namespace Uam.AdvancedProgramming.Api.DTOs.FaultReportStatusLogs;

public record FaultReportStatusLogDto(
    int Id,
    int FaultReportId,
    int ChangedByUserId,
    string ChangedByName,
    string PreviousStatus,
    string NewStatus,
    string? Notes,
    DateTime ChangedAtUtc
);