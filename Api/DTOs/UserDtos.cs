using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs;

// DTO de salida para mostrar usuarios.
public record UserDto(
    int Id,
    int RoleId,
    string RoleName,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive
);

// DTO de entrada para crear usuarios.
public class CreateUserDto
{
    [Required]
    public int RoleId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// DTO de entrada para actualizar usuarios.
public class UpdateUserDto
{
    [Required]
    public int RoleId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}