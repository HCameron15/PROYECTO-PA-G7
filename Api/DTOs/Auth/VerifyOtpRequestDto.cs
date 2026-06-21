using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs.Auth;

public class VerifyOtpRequestDto
{
    [Required]
    public string SessionToken { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
