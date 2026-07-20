using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Uam.AdvancedProgramming.Api.Constants;
using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Services;

public class EmailNotificationService(
    AppDbContext context,
    IEmailService emailService,
    IStringLocalizer<EmailNotificationService> localizer,
    ILogger<EmailNotificationService> logger)
    : IEmailNotificationService
{
    public Task SendReportCreatedAsync(
        FaultReport report,
        CancellationToken cancellationToken = default)
    {
        return ExecuteSafelyAsync(async () =>
        {
            var technicianEmails = await context.Users
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Role.IsActive &&
                    x.Role.Name == RoleNames.Technician)
                .Select(x => x.Email)
                .ToListAsync(cancellationToken);

            foreach (var email in technicianEmails)
            {
                await SendAsync(
                    email,
                    localizer["ReportCreatedSubject", report.Title].Value,
                    BuildBody(report, report.Status, report.ReportedAtUtc),
                    report.Id,
                    cancellationToken);
            }
        }, report.Id);
    }

    public Task SendReportAssignedAsync(
        FaultReport report,
        User technician,
        CancellationToken cancellationToken = default)
    {
        return ExecuteSafelyAsync(() =>
        {
            if (!technician.IsActive)
            {
                return Task.CompletedTask;
            }

            return SendAsync(
                technician.Email,
                localizer["ReportAssignedSubject", report.Title].Value,
                BuildBody(report, report.Status, report.UpdatedAtUtc),
                report.Id,
                cancellationToken);
        }, report.Id);
    }

    public Task SendStatusChangedAsync(
        FaultReport report,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        return ExecuteSafelyAsync(() => SendAsync(
                report.ReportedByUser.Email,
                localizer["StatusChangedSubject", report.Title, newStatus].Value,
                BuildBody(report, newStatus, report.UpdatedAtUtc),
                report.Id,
                cancellationToken),
            report.Id);
    }

    public Task SendReportClosedAsync(
        FaultReport report,
        CancellationToken cancellationToken = default)
    {
        return ExecuteSafelyAsync(() => SendAsync(
                report.ReportedByUser.Email,
                localizer["ReportClosedSubject", report.Title].Value,
                BuildBody(report, report.Status, report.UpdatedAtUtc),
                report.Id,
                cancellationToken),
            report.Id);
    }

    private string BuildBody(
        FaultReport report,
        string status,
        DateTime eventAtUtc)
    {
        return localizer[
            "ReportNotificationBody",
            report.Title,
            report.Equipment.Code,
            report.Equipment.Laboratory?.Name ?? string.Empty,
            status,
            eventAtUtc].Value;
    }

    private async Task SendAsync(
        string recipientEmail,
        string subject,
        string body,
        int faultReportId,
        CancellationToken cancellationToken)
    {
        var sent = await emailService.SendEmailAsync(
            recipientEmail,
            subject,
            body,
            cancellationToken);

        if (!sent)
        {
            logger.LogWarning(
                localizer["NotificationSendFailedLog"].Value,
                faultReportId,
                recipientEmail);
        }
    }

    private async Task ExecuteSafelyAsync(
        Func<Task> notification,
        int faultReportId)
    {
        try
        {
            await notification();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                localizer["NotificationProcessingFailedLog"].Value,
                faultReportId);
        }
    }
}
