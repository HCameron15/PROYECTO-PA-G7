using System.ComponentModel.DataAnnotations;

namespace Uam.AdvancedProgramming.Api.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}