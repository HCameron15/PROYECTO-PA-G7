using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Uam.AdvancedProgramming.MvcClient.Models.FaultReports;

public class UpdateFaultReportViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

    public List<SelectListItem> PriorityOptions { get; set; } =
    [
        new SelectListItem("Low", "Low"),
        new SelectListItem("Medium", "Medium"),
        new SelectListItem("High", "High"),
        new SelectListItem("Critical", "Critical")
    ];

    public List<SelectListItem> StatusOptions { get; set; } =
    [
        new SelectListItem("Pending", "Pending"),
        new SelectListItem("InProgress", "InProgress"),
        new SelectListItem("Resolved", "Resolved")
    ];
}