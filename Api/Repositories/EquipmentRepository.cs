using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class EquipmentRepository(AppDbContext context, IStringLocalizer<EquipmentRepository> localizer)
    : Repository<Equipment>(context), IEquipmentRepository
{
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();

        return await Context.Equipment.AnyAsync(x =>
            x.Code.ToLower() == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSerial = serialNumber.Trim().ToLowerInvariant();

        return await Context.Equipment.AnyAsync(x =>
            x.SerialNumber.ToLower() == normalizedSerial &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<ApiOperationResultDto<List<EquipmentDto>>> GetAllEquipmentAsync(CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<EquipmentDto>>();

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var hasRecords = equipment.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords ? localizer["OperationSuccessful"].Value : localizer["EquipmentNotFound"].Value;
        result.Result = hasRecords ? equipment.Select(MapToDto).ToList() : null;

        return result;
    }

    public async Task<ApiOperationResultDto<EquipmentDto>> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<EquipmentDto>();

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        result.Success = equipment is not null;
        result.Code = equipment is not null ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = equipment is not null ? localizer["OperationSuccessful"].Value : localizer["EquipmentNotFound"].Value;
        result.Result = equipment is null ? null : MapToDto(equipment);

        return result;
    }

    public async Task<ApiOperationResultDto<List<EquipmentDto>>> GetEquipmentByLaboratoryAsync(int laboratoryId, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<EquipmentDto>>();

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .AsNoTracking()
            .Where(x => x.LaboratoryId == laboratoryId)
            .ToListAsync(cancellationToken);

        var hasRecords = equipment.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords ? localizer["OperationSuccessful"].Value : localizer["EquipmentNotFound"].Value;
        result.Result = hasRecords ? equipment.Select(MapToDto).ToList() : null;

        return result;
    }

    public async Task<ApiOperationResultDto<EquipmentDto>> CreateEquipmentAsync(CreateEquipmentDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<EquipmentDto>();

        var laboratory = await Context.Laboratories.FirstOrDefaultAsync(x => x.Id == resource.LaboratoryId, cancellationToken);

        if (laboratory is null || !laboratory.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InactiveLaboratory"].Value;
            return result;
        }

        if (await CodeExistsAsync(resource.Code, null, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["EquipmentCodeExists"].Value;
            return result;
        }

        if (await SerialNumberExistsAsync(resource.SerialNumber, null, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["EquipmentSerialNumberExists"].Value;
            return result;
        }

        var equipment = new Equipment
        {
            LaboratoryId = resource.LaboratoryId,
            Code = resource.Code.Trim(),
            Brand = resource.Brand.Trim(),
            Model = resource.Model.Trim(),
            SerialNumber = resource.SerialNumber.Trim(),
            Type = resource.Type.Trim(),
            Status = resource.Status.Trim(),
            PurchaseDate = resource.PurchaseDate,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Context.Equipment.AddAsync(equipment, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        equipment.Laboratory = laboratory;

        result.Success = true;
        result.Code = StatusCodes.Status201Created.ToString();
        result.Message = localizer["EquipmentCreatedSuccessfully"].Value;
        result.Result = MapToDto(equipment);

        return result;
    }

    public async Task<ApiOperationResultDto<EquipmentDto>> UpdateEquipmentAsync(int id, UpdateEquipmentDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<EquipmentDto>();

        var equipment = await Context.Equipment
            .Include(x => x.Laboratory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipment is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["EquipmentNotFound"].Value;
            return result;
        }

        var laboratory = await Context.Laboratories.FirstOrDefaultAsync(x => x.Id == resource.LaboratoryId, cancellationToken);

        if (laboratory is null || !laboratory.IsActive)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["InactiveLaboratory"].Value;
            return result;
        }

        if (await CodeExistsAsync(resource.Code, id, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["EquipmentCodeExists"].Value;
            return result;
        }

        if (await SerialNumberExistsAsync(resource.SerialNumber, id, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["EquipmentSerialNumberExists"].Value;
            return result;
        }

        equipment.LaboratoryId = resource.LaboratoryId;
        equipment.Code = resource.Code.Trim();
        equipment.Brand = resource.Brand.Trim();
        equipment.Model = resource.Model.Trim();
        equipment.SerialNumber = resource.SerialNumber.Trim();
        equipment.Type = resource.Type.Trim();
        equipment.Status = resource.Status.Trim();
        equipment.PurchaseDate = resource.PurchaseDate;
        equipment.UpdatedAtUtc = DateTime.UtcNow;
        equipment.Laboratory = laboratory;

        Context.Equipment.Update(equipment);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["EquipmentUpdatedSuccessfully"].Value;
        result.Result = MapToDto(equipment);

        return result;
    }

    public async Task<ApiOperationResultDto<object>> DeleteEquipmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        var equipment = await Context.Equipment.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipment is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["EquipmentNotFound"].Value;
            return result;
        }

        equipment.IsActive = false;
        equipment.UpdatedAtUtc = DateTime.UtcNow;

        Context.Equipment.Update(equipment);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["EquipmentDeletedSuccessfully"].Value;

        return result;
    }

    private static EquipmentDto MapToDto(Equipment equipment) =>
        new(
            equipment.Id,
            equipment.LaboratoryId,
            equipment.Laboratory?.Name ?? string.Empty,
            equipment.Code,
            equipment.Brand,
            equipment.Model,
            equipment.SerialNumber,
            equipment.Type,
            equipment.Status,
            equipment.PurchaseDate,
            equipment.IsActive
        );
}