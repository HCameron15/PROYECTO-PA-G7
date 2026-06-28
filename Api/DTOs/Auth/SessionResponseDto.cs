namespace Uam.AdvancedProgramming.Api.DTOs.Auth;

public record SessionResponseDto(
    int Id,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc
);