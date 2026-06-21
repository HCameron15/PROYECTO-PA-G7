namespace Uam.AdvancedProgramming.Api.Models;

public class OtpCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
