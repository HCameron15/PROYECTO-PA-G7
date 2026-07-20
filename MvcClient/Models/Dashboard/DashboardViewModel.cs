namespace Uam.AdvancedProgramming.MvcClient.Models.Dashboard;

public class DashboardViewModel
{
    public GeneralSummaryViewModel GeneralSummary { get; set; } = new();

    public List<ReportsByLabViewModel> ReportsByLab { get; set; } = [];

    public List<ReportsByStatusViewModel> ReportsByStatus { get; set; } = [];

    public List<ReportsByTechnicianViewModel> ReportsByTechnician { get; set; } = [];

    public AverageResolutionTimeViewModel AverageResolutionTime { get; set; } = new();

    public string? ErrorMessage { get; set; }
}