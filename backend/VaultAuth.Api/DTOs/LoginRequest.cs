using System.ComponentModel.DataAnnotations;

namespace VaultAuth.Api.DTOs
{
    public class LoginRequest
    {

       [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }


}
