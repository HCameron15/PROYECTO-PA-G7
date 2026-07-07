using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class CreateFaultReportViewModel
{
    [Required]
    public int EquipmentId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = string.Empty;

    public List<SelectListItem> EquipmentOptions { get; set; } = [];

    public List<SelectListItem> PriorityOptions { get; set; } =
    [
        new SelectListItem("Low", "Low"),
        new SelectListItem("Medium", "Medium"),
        new SelectListItem("High", "High"),
        new SelectListItem("Critical", "Critical")
    ];
}