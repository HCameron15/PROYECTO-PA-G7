using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs;

// DTO de salida para mostrar estudiantes en respuestas GET/POST/PUT.
public record StudentDto(int Id, string FirstName, string LastName, string Email, DateOnly BirthDate);

// DTO de entrada para crear un estudiante.
public class CreateStudentDto
{
    // Requerido y con tamaño máximo para evitar datos inválidos.
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    // Requerido y con tamaño máximo para evitar datos inválidos.
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    // Requerido, formato correo y longitud máxima.
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    // Requerido para que siempre exista fecha de nacimiento.
    [Required]
    public DateOnly BirthDate { get; set; }
}

// DTO de entrada para actualizar un estudiante existente.
public class UpdateStudentDto
{
    // Requerido y con tamaño máximo para evitar datos inválidos.
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    // Requerido y con tamaño máximo para evitar datos inválidos.
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    // Requerido, formato correo y longitud máxima.
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    // Requerido para que siempre exista fecha de nacimiento.
    [Required]
    public DateOnly BirthDate { get; set; }
}
