namespace Uam.AdvancedProgramming.Api.Models;

public class PendingLoginSession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public Guid SessionToken { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
