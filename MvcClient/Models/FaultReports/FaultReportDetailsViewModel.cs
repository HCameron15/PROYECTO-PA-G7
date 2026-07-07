namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class FaultReportDetailsViewModel
{
    public FaultReportDto Report { get; set; } = new();

    public List<FaultReportStatusLogDto> Logs { get; set; } = [];
}