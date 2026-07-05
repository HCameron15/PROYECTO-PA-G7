using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs.FaultReports;

public class UpdateFaultReportDto
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;
}