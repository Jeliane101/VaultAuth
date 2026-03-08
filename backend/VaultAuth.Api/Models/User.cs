using System.ComponentModel.DataAnnotations;

namespace VaultAuth.Api.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string HashedPassword { get; set; } = string.Empty;

        public int FailedLognAtp { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        public string? ImageURL { get; set; }
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

    }
}
