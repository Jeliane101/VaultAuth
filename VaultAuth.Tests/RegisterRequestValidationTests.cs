using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaultAuth.Api.DTOs;

namespace VaultAuth.Tests
{
    public class RegisterRequestValidationTests
    {
        private static IList<ValidationResult> Validate(RegisterRequest request)
        {
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(request, context, results, true);
            return results;
        }

        [Fact]
        public void ShouldFail_WhenPasswordIsTooShort()
        {
            var request = new RegisterRequest { FirstName = "John", LastName = "Smith", Email = "user@example.com", Password = "123" };
            var results = Validate(request);

            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void ShouldFail_WhenPasswordDoesNotMeetComplexity()
        {
            var request = new RegisterRequest { FirstName = "John", LastName = "Smith", Email = "user@example.com", Password = "password" };
            var results = Validate(request);

            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void ShouldPass_WhenAllFieldsAreValid()
        {
            var request = new RegisterRequest { FirstName = "John", LastName = "Smith", Email = "user@example.com", Password = "StrongPass123!" };
            var results = Validate(request);

            Assert.Empty(results); 
        }
    }
}
