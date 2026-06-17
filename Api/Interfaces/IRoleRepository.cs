using Uam.AdvancedProgramming.Api.DTOs;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IRoleRepository
{
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<List<RoleDto>>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<RoleDto>> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<RoleDto>> CreateRoleAsync(CreateRoleDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<RoleDto>> UpdateRoleAsync(int id, UpdateRoleDto resource, CancellationToken cancellationToken = default);

    Task<ApiOperationResultDto<object>> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);
}