using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.Models;

public class Equipment
{
    public int Id { get; set; }

    [Required]
    public int LaboratoryId { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SerialNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Laboratory? Laboratory { get; set; }

    public ICollection<FaultReport> FaultReports { get; set; } = [];
}