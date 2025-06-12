using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;

namespace SafeNodeAPI.Tests.IntegrationTests
{
    public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact(DisplayName = "Register + Login Flow Should Return Valid JWT + RefreshToken")]
        public async Task RegisterAndLogin_ShouldReturnJwtAndRefreshToken()
        {
            // Arrange
            var email = $"testuser_{Guid.NewGuid()}@example.com";
            var password = "StrongPass@123";

            var registerRequest = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = password,
            };

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act - Register
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/registerUser", registerRequest);
            registerResponse.EnsureSuccessStatusCode();
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

            // Assert Registration
            registerResult.Should().NotBeNull();
            registerResult!.Email.Should().Be(email);
            registerResult.UserId.Should().BeGreaterThan(0);

            // Act - Login
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // Assert Login
            loginResult.Should().NotBeNull();
            loginResult!.Token.Should().NotBeNullOrWhiteSpace();
            loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
            loginResult.Expiry.Should().BeAfter(DateTime.UtcNow);
            loginResult.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);
            loginResult.Message.Should().Be("Logged in Successfully");

            // Parse and validate JWT
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(loginResult.Token);

            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.NameId);
        }

        [Fact(DisplayName = "Register with existing email should return conflict")]
        public async Task Register_WithExistingEmail_ShouldReturnConflict()
        {
            // Arrange
            var email = $"existinguser_{Guid.NewGuid()}@example.com";
            var password = "StrongPass@123";

            var registerRequest = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = password,
            };

            // First registration (should succeed)
            var firstRegisterResponse = await _client.PostAsJsonAsync("/api/auth/registerUser", registerRequest);
            firstRegisterResponse.EnsureSuccessStatusCode();

            // Second registration attempt (should fail)
            var secondRegisterResponse = await _client.PostAsJsonAsync("/api/auth/registerUser", registerRequest);

            // Assert
            secondRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact(DisplayName = "Login with invalid credentials should return unauthorized")]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "wrongpassword"
            };

            // Act
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}