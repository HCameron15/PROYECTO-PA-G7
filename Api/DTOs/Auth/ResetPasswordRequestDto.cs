using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required]
    public string SessionToken { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}