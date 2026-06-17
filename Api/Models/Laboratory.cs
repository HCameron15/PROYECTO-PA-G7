using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.Models;

public class Laboratory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Building { get; set; } = string.Empty;

    [Required]
    public int Floor { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<Equipment>? Equipment { get; set; }
}
