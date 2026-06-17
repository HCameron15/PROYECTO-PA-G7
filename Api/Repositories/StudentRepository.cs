using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;
using Uam.AdvancedProgramming.Api.Data;

namespace Uam.AdvancedProgramming.Api.Repositories;

/// <summary>
/// Repositorio especializado con reglas de negocio del módulo de estudiantes.
/// </summary>
public class StudentRepository(AppDbContext context, IStringLocalizer<StudentRepository> localizer) : Repository<Student>(context), IStudentRepository
{
    /// <summary>
    /// Verifica existencia de correo, opcionalmente excluyendo un registro por id.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await Context.Students.AnyAsync(x => x.Email == normalizedEmail && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    /// <summary>
    /// Obtiene la lista de estudiantes y la empaqueta en formato estándar de respuesta.
    /// </summary>
    public async Task<ApiOperationResultDto<List<StudentDto>>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<StudentDto>>();
        var students = await Context.Students.AsNoTracking().ToListAsync(cancellationToken);
        var hasRecords = students.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords ? localizer["OperationSuccessful"].Value : localizer["StudentsNotFound"].Value;
        result.Result = hasRecords ? students.Select(MapToDto).ToList() : null;

        return result;
    }

    /// <summary>
    /// Obtiene un estudiante por id y lo empaqueta en formato estándar de respuesta.
    /// </summary>
    public async Task<ApiOperationResultDto<StudentDto>> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<StudentDto>();
        var student = await Context.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        result.Success = student is not null;
        result.Code = student is not null ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = student is not null ? localizer["OperationSuccessful"].Value : localizer["StudentNotFound"].Value;
        result.Result = student is null ? null : MapToDto(student);

        return result;
    }

    /// <summary>
    /// Crea un estudiante validando duplicidad de correo.
    /// </summary>
    public async Task<ApiOperationResultDto<StudentDto>> CreateStudentAsync(CreateStudentDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<StudentDto>();
        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();

        if (await EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["StudentEmailExists"].Value;
            return result;
        }

        var student = new Student
        {
            FirstName = resource.FirstName.Trim(),
            LastName = resource.LastName.Trim(),
            Email = normalizedEmail,
            BirthDate = resource.BirthDate,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Context.Students.AddAsync(student, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status201Created.ToString();
        result.Message = localizer["StudentCreatedSuccessfully"].Value;
        result.Result = MapToDto(student);
        return result;
    }

    /// <summary>
    /// Actualiza un estudiante validando existencia y duplicidad de correo.
    /// </summary>
    public async Task<ApiOperationResultDto<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<StudentDto>();
        var student = await Context.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["StudentNotFound"].Value;
            return result;
        }

        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();
        if (await EmailExistsAsync(normalizedEmail, id, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["StudentEmailExists"].Value;
            return result;
        }

        student.FirstName = resource.FirstName.Trim();
        student.LastName = resource.LastName.Trim();
        student.Email = normalizedEmail;
        student.BirthDate = resource.BirthDate;
        student.UpdatedAtUtc = DateTime.UtcNow;

        Context.Students.Update(student);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["StudentUpdatedSuccessfully"].Value;
        result.Result = MapToDto(student);
        return result;
    }

    /// <summary>
    /// Elimina un estudiante por id.
    /// </summary>
    public async Task<ApiOperationResultDto<object>> DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();
        var student = await Context.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["StudentNotFound"].Value;
            return result;
        }

        Context.Students.Remove(student);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["StudentDeletedSuccessfully"].Value;
        return result;
    }

    /// <summary>
    /// Convierte la entidad Student a su DTO de salida.
    /// </summary>
    private static StudentDto MapToDto(Student s) => new(s.Id, s.FirstName, s.LastName, s.Email, s.BirthDate);
}
