using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Interfaces;

/// <summary>
/// Contrato especializado del repositorio de estudiantes.
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    /// <summary>
    /// Verifica si ya existe un correo registrado, opcionalmente excluyendo un id.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtiene todos los estudiantes con formato estándar de respuesta API.
    /// </summary>
    Task<ApiOperationResultDto<List<StudentDto>>> GetAllStudentsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtiene un estudiante por id con formato estándar de respuesta API.
    /// </summary>
    Task<ApiOperationResultDto<StudentDto>> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Crea un estudiante con formato estándar de respuesta API.
    /// </summary>
    Task<ApiOperationResultDto<StudentDto>> CreateStudentAsync(CreateStudentDto resource, CancellationToken cancellationToken = default);
    /// <summary>
    /// Actualiza un estudiante con formato estándar de respuesta API.
    /// </summary>
    Task<ApiOperationResultDto<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto resource, CancellationToken cancellationToken = default);
    /// <summary>
    /// Elimina un estudiante con formato estándar de respuesta API.
    /// </summary>
    Task<ApiOperationResultDto<object>> DeleteStudentAsync(int id, CancellationToken cancellationToken = default);
}
