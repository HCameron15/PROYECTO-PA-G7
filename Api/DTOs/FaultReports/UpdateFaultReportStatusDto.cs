using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs.FaultReports;

public class UpdateFaultReportStatusDto
{
    [Required]
    public string NewStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}