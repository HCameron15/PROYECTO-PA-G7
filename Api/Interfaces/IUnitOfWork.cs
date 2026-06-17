namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IUnitOfWork
{
    IStudentRepository Students { get; }

    ILaboratoryRepository Laboratories { get; }

    IEquipmentRepository Equipment { get; }

    IRoleRepository Roles { get; }

    IUserRepository Users { get; }

    IAuthRepository Auth { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}