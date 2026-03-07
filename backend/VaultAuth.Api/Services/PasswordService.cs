using Microsoft.AspNetCore.Identity;
using VaultAuth.Api.Models;

namespace VaultAuth.Api.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
