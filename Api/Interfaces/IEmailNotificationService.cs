using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IEmailNotificationService
{
    Task SendReportCreatedAsync(
        FaultReport report,
        CancellationToken cancellationToken = default);

    Task SendReportAssignedAsync(
        FaultReport report,
        User technician,
        CancellationToken cancellationToken = default);

    Task SendStatusChangedAsync(
        FaultReport report,
        string newStatus,
        CancellationToken cancellationToken = default);

    Task SendReportClosedAsync(
        FaultReport report,
        CancellationToken cancellationToken = default);
}
