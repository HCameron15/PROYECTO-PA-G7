namespace Uam.AdvancedProgramming.MvcClient.Models.Dashboard;

public class ReportsByLabViewModel
{
    public int LabId { get; set; }

    public string LabName { get; set; } = string.Empty;

    public int TotalReports { get; set; }

    public int PendingCount { get; set; }

    public int InProgressCount { get; set; }
}