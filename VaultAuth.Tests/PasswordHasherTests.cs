using System;
using System.Collections.Generic;
using System.Text;
using VaultAuth.Api.Models;
using VaultAuth.Api.Services;

namespace VaultAuth.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_ShouldReturnDifferentHash_ForSamePassword()
        {
            var service = new PasswordService();
            var user = new User { Email = "test@example.com" }; // dummy user

            var hash1 = service.HashPassword(user, "MySecret123!");
            var hash2 = service.HashPassword(user, "MySecret123!");

            Assert.NotEqual(hash1, hash2); // salted hashes should differ
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            var service = new PasswordService();
            var user = new User { Email = "test@example.com" };

            var password = "MySecret123!";
            user.HashedPassword = service.HashPassword(user, password);

            Assert.True(service.VerifyPassword(user, password));
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForWrongPassword()
        {
            var service = new PasswordService();
            var user = new User { Email = "test@example.com" };

            user.HashedPassword = service.HashPassword(user, "MySecret123!");

            Assert.False(service.VerifyPassword(user, "WrongPassword!"));
        }
    }
}
