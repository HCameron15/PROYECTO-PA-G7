using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;

namespace Uam.AdvancedProgramming.Api.Controllers;

/// <summary>
/// Controlador API para administrar operaciones CRUD de estudiantes.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StudentsController(IUnitOfWork unitOfWork, IStringLocalizer<StudentsController> stringLocalizer) : ControllerBase
{
    /// <summary>
    /// Obtiene la lista completa de estudiantes.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Resultado de operación con lista de estudiantes o mensaje de no encontrados.</returns>
    [HttpGet(nameof(GetAllStudents))]
    [ProducesResponseType(typeof(ApiOperationResultDto<List<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllStudents(CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Students.GetAllStudentsAsync(cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Obtiene un estudiante específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Resultado con el estudiante encontrado o respuesta 404.</returns>
    [HttpGet(nameof(GetStudentById) + "/{id:int}")]
    [ProducesResponseType(typeof(ApiOperationResultDto<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Students.GetStudentByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Crea un nuevo estudiante en la base de datos.
    /// </summary>
    /// <param name="resource">Datos necesarios para crear el estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Resultado con el estudiante creado o error de validación.</returns>
    [HttpPost(nameof(CreateStudent))]
    [ProducesResponseType(typeof(ApiOperationResultDto<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Students.CreateStudentAsync(resource, cancellationToken);
        return result.Success ? Created(string.Empty, result) : BadRequest(result);
    }

    /// <summary>
    /// Actualiza los datos de un estudiante existente.
    /// </summary>
    /// <param name="id">Identificador del estudiante a actualizar.</param>
    /// <param name="resource">Nuevos datos del estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Resultado con estudiante actualizado o mensaje de error.</returns>
    [HttpPut(nameof(UpdateStudent) + "/{id:int}")]
    [ProducesResponseType(typeof(ApiOperationResultDto<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto resource, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiOperationResultDto<object>
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = stringLocalizer["InvalidModel"].Value
            });
        }

        var result = await unitOfWork.Students.UpdateStudentAsync(id, resource, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return result.Code == StatusCodes.Status404NotFound.ToString() ? NotFound(result) : BadRequest(result);
    }

    /// <summary>
    /// Elimina un estudiante por su identificador.
    /// </summary>
    /// <param name="id">Identificador del estudiante a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Resultado de éxito o respuesta 404 si no existe.</returns>
    [HttpDelete(nameof(DeleteStudent) + "/{id:int}")]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiOperationResultDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Students.DeleteStudentAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
