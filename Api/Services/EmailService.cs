using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Uam.AdvancedProgramming.Api.Interfaces;
using Uam.AdvancedProgramming.Api.Models.Configurations;

namespace Uam.AdvancedProgramming.Api.Services;

public class EmailService(
    IOptions<SmtpSettings> smtpOptions
) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpOptions.Value;

    public async Task<bool> SendOtpAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _smtpSettings.SenderName,
                _smtpSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Código OTP - UAM Lab Help Desk";

            message.Body = new TextPart("plain")
            {
                Text = $"Su código OTP es: {otpCode}. Este código vence en 10 minutos."
            };

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
        catch 
        {
            //Console.WriteLine("SMTP ERROR:");
            //Console.WriteLine(ex.Message);
            //Console.WriteLine(ex.InnerException?.Message);

            return false;
        }
    }
}