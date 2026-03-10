using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaultAuth.Api.Models;

namespace VaultAuth.Tests
{
    public class UserValidationTests
    {
        [Fact]
        public void User_ShouldFailValidation_WhenEmailIsMissing()
        {
            var user = new User { HashedPassword = "hashed" };
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void User_ShouldFailValidation_WhenEmailIsInvalid()
        {
            var user = new User { Email = "not-an-email", HashedPassword = "hashed" };
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void User_ShouldPassValidation_WhenAllFieldsAreValid()
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "user@example.com",
                HashedPassword = "hashed"
            };

            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.True(isValid);
        }

        [Fact]
        public void User_ShouldFailValidation_WhenPasswordIsMissing()
        {
            var user = new User { Email = "user@example.com" };
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("HashedPassword"));
        }

        [Fact]
        public void User_ShouldFailValidation_WhenFirstNameIsMissing()
        {
            var user = new User { LastName = "Smith", Email = "user@example.com", HashedPassword = "hashed" };
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void User_ShouldFailValidation_WhenLastNameIsMissing()
        {
            var user = new User { FirstName = "John", Email = "user@example.com", HashedPassword = "hashed" };
            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }




    }
}
