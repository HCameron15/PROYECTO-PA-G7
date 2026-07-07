namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class FaultReportStatusLogDto
{
    public int Id { get; set; }
    public int FaultReportId { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}