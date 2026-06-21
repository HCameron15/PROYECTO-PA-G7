namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default);
}