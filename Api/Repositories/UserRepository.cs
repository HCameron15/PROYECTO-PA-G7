using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.DTOs;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class UserRepository(AppDbContext context, IStringLocalizer<UserRepository> localizer) : Repository<User>(context), IUserRepository
{
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await Context.Users.AnyAsync(
            x => x.Email == normalizedEmail && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<ApiOperationResultDto<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<UserDto>>();

        var users = await Context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .ToListAsync(cancellationToken);

        var hasRecords = users.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords ? localizer["OperationSuccessful"].Value : localizer["UsersNotFound"].Value;
        result.Result = hasRecords ? users.Select(MapToDto).ToList() : null;

        return result;
    }

    public async Task<ApiOperationResultDto<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<UserDto>();

        var user = await Context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        result.Success = user is not null;
        result.Code = user is not null ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = user is not null ? localizer["OperationSuccessful"].Value : localizer["UserNotFound"].Value;
        result.Result = user is null ? null : MapToDto(user);

        return result;
    }

    public async Task<ApiOperationResultDto<List<UserDto>>> GetUsersByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<List<UserDto>>();

        var users = await Context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken);

        var hasRecords = users.Count > 0;

        result.Success = hasRecords;
        result.Code = hasRecords ? StatusCodes.Status200OK.ToString() : StatusCodes.Status404NotFound.ToString();
        result.Message = hasRecords ? localizer["OperationSuccessful"].Value : localizer["UsersNotFound"].Value;
        result.Result = hasRecords ? users.Select(MapToDto).ToList() : null;

        return result;
    }

    public async Task<ApiOperationResultDto<UserDto>> CreateUserAsync(CreateUserDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<UserDto>();
        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();

        if (await EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["UserEmailExists"].Value;
            return result;
        }

        var role = await Context.Roles.FirstOrDefaultAsync(
            x => x.Id == resource.RoleId && x.IsActive,
            cancellationToken);

        if (role is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["ActiveRoleRequired"].Value;
            return result;
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            RoleId = resource.RoleId,
            FirstName = resource.FirstName.Trim(),
            LastName = resource.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(resource.Password),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await Context.Users.AddAsync(user, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        user.Role = role;

        result.Success = true;
        result.Code = StatusCodes.Status201Created.ToString();
        result.Message = localizer["UserCreatedSuccessfully"].Value;
        result.Result = MapToDto(user);

        return result;
    }

    public async Task<ApiOperationResultDto<UserDto>> UpdateUserAsync(int id, UpdateUserDto resource, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<UserDto>();

        var user = await Context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["UserNotFound"].Value;
            return result;
        }

        var normalizedEmail = resource.Email.Trim().ToLowerInvariant();

        if (await EmailExistsAsync(normalizedEmail, id, cancellationToken))
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["UserEmailExists"].Value;
            return result;
        }

        var role = await Context.Roles.FirstOrDefaultAsync(
            x => x.Id == resource.RoleId && x.IsActive,
            cancellationToken);

        if (role is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status400BadRequest.ToString();
            result.Message = localizer["ActiveRoleRequired"].Value;
            return result;
        }

        user.RoleId = resource.RoleId;
        user.FirstName = resource.FirstName.Trim();
        user.LastName = resource.LastName.Trim();
        user.Email = normalizedEmail;

        if (!string.IsNullOrWhiteSpace(resource.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resource.Password);
        }

        user.UpdatedAtUtc = DateTime.UtcNow;

        Context.Users.Update(user);
        await Context.SaveChangesAsync(cancellationToken);

        user.Role = role;

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["UserUpdatedSuccessfully"].Value;
        result.Result = MapToDto(user);

        return result;
    }

    public async Task<ApiOperationResultDto<object>> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = new ApiOperationResultDto<object>();

        var user = await Context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            result.Success = false;
            result.Code = StatusCodes.Status404NotFound.ToString();
            result.Message = localizer["UserNotFound"].Value;
            return result;
        }

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;

        Context.Users.Update(user);
        await Context.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.Code = StatusCodes.Status200OK.ToString();
        result.Message = localizer["UserDeletedSuccessfully"].Value;

        return result;
    }

    private static UserDto MapToDto(User user) =>
        new(
            user.Id,
            user.RoleId,
            user.Role.Name,
            user.FirstName,
            user.LastName,
            user.Email,
            user.IsActive
        );
}