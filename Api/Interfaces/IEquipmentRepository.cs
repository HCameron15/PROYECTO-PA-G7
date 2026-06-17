using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IEquipmentRepository : IRepository<Equipment>
{
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<EquipmentDto>>> GetAllEquipmentAsync(CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<EquipmentDto>> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<EquipmentDto>>> GetEquipmentByLaboratoryAsync(int laboratoryId, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<EquipmentDto>> CreateEquipmentAsync(CreateEquipmentDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<EquipmentDto>> UpdateEquipmentAsync(int id, UpdateEquipmentDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> DeleteEquipmentAsync(int id, CancellationToken cancellationToken = default);
}