using Microsoft.AspNetCore.Identity;
using VaultAuth.Api.Models;

namespace VaultAuth.Api.Services
{
    public interface IPasswordService
    {
        string HashPassword(User user, string password);
        bool VerifyPassword(User user, string password);
    }
}
