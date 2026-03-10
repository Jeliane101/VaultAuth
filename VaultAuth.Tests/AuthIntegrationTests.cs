using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VaultAuth.Api;
using VaultAuth.Api.DTOs;


namespace VaultAuth.Tests
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Login_GetUser_Flow_Works()
        {
            var email = $"johndoe_{Guid.NewGuid()}@example.com";
            var password = "StrongPass123!";

            // Register
            var formData = new MultipartFormDataContent
            {
             { new StringContent("John"), "FirstName" },
             { new StringContent("Doe"), "LastName" },
             { new StringContent(email), "Email" },
             { new StringContent(password), "Password" }

            };

            var registerResponse = await _client.PostAsync("/api/auth/register", formData);
            registerResponse.EnsureSuccessStatusCode();

            // Login
            var loginPayload = new { Email = email, Password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            loginResponse.EnsureSuccessStatusCode();

            var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string token = loginContent.GetProperty("accessToken").GetString();
            Assert.False(string.IsNullOrEmpty(token));

            //GetUser
            _client.DefaultRequestHeaders.Authorization =
             new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var userResponse = await _client.GetAsync("/api/auth/profile");
            userResponse.EnsureSuccessStatusCode();

            var userContent = await userResponse.Content.ReadFromJsonAsync<JsonElement>();
            string emailFromProfile = userContent.GetProperty("email").GetString();
            Assert.Equal(email, emailFromProfile);

        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldFail()
        {
            var email = $"duplicate_{Guid.NewGuid()}@example.com";
            var password = "StrongPass123!";

            // First registration succeeds
            var formData1 = new MultipartFormDataContent
    {
        { new StringContent("Jane"), "FirstName" },
        { new StringContent("Doe"), "LastName" },
        { new StringContent(email), "Email" },
        { new StringContent(password), "Password" }
    };
            var firstResponse = await _client.PostAsync("/api/auth/register", formData1);
            firstResponse.EnsureSuccessStatusCode();

            // Second registration with same email should fail
            var formData2 = new MultipartFormDataContent
    {
        { new StringContent("Jane"), "FirstName" },
        { new StringContent("Doe"), "LastName" },
        { new StringContent(email), "Email" },
        { new StringContent(password), "Password" }
    };
            var secondResponse = await _client.PostAsync("/api/auth/register", formData2);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShouldFail()
        {
            var email = $"Wtestcase_{Guid.NewGuid()}@example.com";
            var password = "StrongPass123!";

            // Register user
            var formData = new MultipartFormDataContent
    {
        { new StringContent("John"), "FirstName" },
        { new StringContent("Doe"), "LastName" },
        { new StringContent(email), "Email" },
        { new StringContent(password), "Password" }
    };
            var registerResponse = await _client.PostAsync("/api/auth/register", formData);
            registerResponse.EnsureSuccessStatusCode();

            // Attempt login with wrong password
            var loginPayload = new { Email = email, Password = "WrongPassword!" };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Login_ThreeFailedAttempts_ShouldLockAccount()
        {
            var email = $"ltestcase_{Guid.NewGuid()}@example.com";
            var password = "StrongPass123!";

            // Register user
            var formData = new MultipartFormDataContent
    {
        { new StringContent("Lock"), "FirstName" },
        { new StringContent("Out"), "LastName" },
        { new StringContent(email), "Email" },
        { new StringContent(password), "Password" }
    };
            var registerResponse = await _client.PostAsync("/api/auth/register", formData);
            registerResponse.EnsureSuccessStatusCode();

            // Fail login 3 times
            for (int i = 0; i < 3; i++)
            {
                var loginPayload = new { Email = email, Password = "WrongPassword!" };
                await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            }

            // Fourth attempt should return lockout info
            var lockedResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, lockedResponse.StatusCode);

            var content = await lockedResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(content.TryGetProperty("lockoutEnd", out _));
        }

    }
}
