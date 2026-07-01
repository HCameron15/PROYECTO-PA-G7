namespace Uam.AdvancedProgramming.Api.DTOs.Auth;

public record ForgotPasswordResponseDto(
    string Message,
    string? SessionToken);