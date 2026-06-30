namespace Uam.AdvancedProgramming.Api.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken cancellationToken = default);

    Task<bool> SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        string resetLink,
        CancellationToken cancellationToken = default);
}