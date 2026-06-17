namespace Uam.AdvancedProgramming.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public Role Role { get; set; } = null!;
    }

}
