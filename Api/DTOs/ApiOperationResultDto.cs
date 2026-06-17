namespace Uam.AdvancedProgramming.Api.DTOs;

// DTO genérico para estandarizar respuestas de la API.
// "T" representa el tipo de dato que regresará cada operación.
/// <summary>
/// Estructura estándar de respuesta para todos los endpoints.
/// </summary>
public class ApiOperationResultDto<T>
{
    // Indica si la operación fue exitosa.
    /// <summary>
    /// Indica si la operación fue satisfactoria.
    /// </summary>
    public bool Success { get; set; }

    // Código HTTP guardado como texto (200, 404, 400, etc.).
    /// <summary>
    /// Código HTTP de la operación expresado como texto.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    // Mensaje legible para la persona usuaria.
    /// <summary>
    /// Mensaje descriptivo de la operación para la persona usuaria.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    // Resultado de la operación (puede ser null si falla o no hay datos).
    /// <summary>
    /// Datos devueltos por la operación. Puede ser nulo cuando no hay resultado.
    /// </summary>
    public T? Result { get; set; }
}
