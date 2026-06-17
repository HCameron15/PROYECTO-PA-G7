using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs;

public record EquipmentDto(
    int Id,
    int LaboratoryId,
    string LaboratoryName,
    string Code,
    string Brand,
    string Model,
    string SerialNumber,
    string Type,
    string Status,
    DateOnly? PurchaseDate,
    bool IsActive
);

public class CreateEquipmentDto
{
    [Required]
    public int LaboratoryId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string SerialNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }
}

public class UpdateEquipmentDto
{
    [Required]
    public int LaboratoryId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string SerialNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }
}