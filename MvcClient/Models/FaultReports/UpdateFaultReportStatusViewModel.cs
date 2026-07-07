using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class UpdateFaultReportStatusViewModel
{
    public int FaultReportId { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    [Required]
    public string NewStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public List<string> ValidStatuses { get; set; } = [];
}