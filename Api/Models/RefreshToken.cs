namespace Uam.AdvancedProgramming.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedReason { get; set; }
}