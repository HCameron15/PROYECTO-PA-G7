namespace Uam.AdvancedProgramming.MvcClient.Models.Dashboard;

public class ReportsByTechnicianViewModel
{
    public int TechnicianId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int AssignedCount { get; set; }

    public int ResolvedCount { get; set; }
}