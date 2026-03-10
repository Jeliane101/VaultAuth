using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VaultAuth.Api.Data;
using VaultAuth.Api.Models;

namespace VaultAuth.Tests
{
    public class DuplicateEmailTests
    {
        [Fact]
        public void ShouldDetectDuplicateEmail()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("DuplicateEmailDb")
                .Options;

            using (var context = new AppDbContext(options))
            {
                // first user
                context.Users.Add(new User
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "test@example.com",
                    HashedPassword = "hashed"
                });
                context.SaveChanges();

                // second user with the same email
                var duplicateUser = new User
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "test@example.com",
                    HashedPassword = "hashed2"
                };

                bool emailExists = context.Users.Any(u => u.Email == duplicateUser.Email);

                Assert.True(emailExists); 
            }
        }

        [Fact]
        public void ShouldAllowUniqueEmail()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("UniqueEmailDb")
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "test@example.com",
                    HashedPassword = "hashed"
                });
                context.SaveChanges();

                var newUser = new User
                {
                    FirstName = "Alice",
                    LastName = "Brown",
                    Email = "unique@example.com",
                    HashedPassword = "hashed2"
                };

                bool emailExists = context.Users.Any(u => u.Email == newUser.Email);

                Assert.False(emailExists); 
            }
        }
        }
}
