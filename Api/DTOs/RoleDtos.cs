using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs;

// DTO de salida para mostrar roles.
public record RoleDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);

// DTO de entrada para crear un rol.
public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }
}

// DTO de entrada para actualizar un rol.
public class UpdateRoleDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }
}