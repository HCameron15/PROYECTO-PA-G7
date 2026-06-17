namespace Uam.AdvancedProgramming.Api.Models;

// Esta clase representa la tabla de estudiantes en la base de datos.
/// <summary>
/// Entidad de dominio que modela la información principal de un estudiante.
/// </summary>
public class Student
{
    // Identificador único del estudiante (llave primaria).
    /// <summary>
    /// Llave primaria de la tabla de estudiantes.
    /// </summary>
    public int Id { get; set; }

    // Nombre del estudiante.
    /// <summary>
    /// Nombre del estudiante.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    // Apellido del estudiante.
    /// <summary>
    /// Apellido del estudiante.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    // Correo electrónico del estudiante (debe ser único).
    /// <summary>
    /// Correo electrónico único del estudiante.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    // Fecha de nacimiento del estudiante.
    /// <summary>
    /// Fecha de nacimiento del estudiante.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    // Fecha/hora UTC de creación del registro.
    /// <summary>
    /// Fecha y hora UTC de creación del registro.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    // Fecha/hora UTC de última actualización del registro.
    /// <summary>
    /// Fecha y hora UTC de la última actualización del registro.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
