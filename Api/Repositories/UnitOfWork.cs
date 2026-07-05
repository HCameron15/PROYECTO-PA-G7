using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Services;

namespace Uam.AdvancedProgramming.Api.Repositories;

public class UnitOfWork(
    AppDbContext context,
    IConfiguration configuration,
    IStringLocalizer<StudentRepository> studentLocalizer,
    IStringLocalizer<LaboratoryRepository> laboratoryLocalizer,
    IStringLocalizer<EquipmentRepository> equipmentLocalizer,
    IStringLocalizer<RoleRepository> roleLocalizer,
    IStringLocalizer<UserRepository> userLocalizer,
    IStringLocalizer<AuthRepository> authLocalizer,
    IStringLocalizer<FaultReportRepository> faultReportLocalizer,
    IEmailService emailService
) : IUnitOfWork
{
    private IStudentRepository? _students;
    private ILaboratoryRepository? _laboratories;
    private IEquipmentRepository? _equipment;
    private IRoleRepository? _roles;
    private IUserRepository? _users;
    private IAuthRepository? _auth;
    private IFaultReportRepository? _faultReports;

    public IStudentRepository Students =>
        _students ??= new StudentRepository(
            context,
            studentLocalizer);

    public ILaboratoryRepository Laboratories =>
        _laboratories ??= new LaboratoryRepository(
            context,
            laboratoryLocalizer);

    public IEquipmentRepository Equipment =>
        _equipment ??= new EquipmentRepository(
            context,
            equipmentLocalizer);

    public IRoleRepository Roles =>
        _roles ??= new RoleRepository(
            context,
            roleLocalizer);

    public IUserRepository Users =>
        _users ??= new UserRepository(
            context,
            userLocalizer);

    public IAuthRepository Auth =>
        _auth ??= new AuthRepository(
            context,
            configuration,
            authLocalizer,
            emailService);

    public IFaultReportRepository FaultReports =>
        _faultReports ??= new FaultReportRepository(
            context,
            faultReportLocalizer);

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}