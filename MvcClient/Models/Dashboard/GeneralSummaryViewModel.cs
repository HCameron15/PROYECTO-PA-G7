namespace Uam.AdvancedProgramming.MvcClient.Models.Dashboard;

public class GeneralSummaryViewModel
{
    public int TotalReports { get; set; }

    public int PendingCount { get; set; }

    public int InProgressCount { get; set; }

    public int ResolvedCount { get; set; }

    public int ClosedCount { get; set; }

    public int TotalEquipment { get; set; }

    public int EquipmentUnderRepair { get; set; }
}