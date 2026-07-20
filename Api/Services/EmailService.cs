using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using MimeKit;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models.Configurations;

namespace Uam.AdvancedProgramming.Api.Services;

public class EmailService(
    IOptions<SmtpSettings> smtpOptions,
    IStringLocalizer<EmailService> localizer,
    ILogger<EmailService> logger
) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpOptions.Value;

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _smtpSettings.SenderName,
                _smtpSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _smtpSettings.Host,
                _smtpSettings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
                _smtpSettings.SenderEmail,
                _smtpSettings.Password,
                cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                localizer["EmailSendErrorLog"].Value,
                toEmail);

            return false;
        }
    }

    public async Task<bool> SendOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(
            toEmail,
            localizer["OtpSubject"].Value,
            localizer["OtpBody", otpCode].Value,
            cancellationToken);
    }

    public async Task<bool> SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(
            toEmail,
            localizer["PasswordResetSubject"].Value,
            localizer["PasswordResetBody", otpCode, resetLink].Value,
            cancellationToken);
    }
}
