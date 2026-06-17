using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs;

// DTO de salida
public record LaboratoryDto(
    int Id,
    string Name,
    string Building,
    int Floor,
    int Capacity,
    bool IsActive
);

// DTO para crear
public class CreateLaboratoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Building { get; set; } = string.Empty;

    [Required]
    public int Floor { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}

// DTO para actualizar
public class UpdateLaboratoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Building { get; set; } = string.Empty;

    [Required]
    public int Floor { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}