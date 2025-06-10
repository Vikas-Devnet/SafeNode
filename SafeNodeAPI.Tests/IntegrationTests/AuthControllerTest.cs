using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace SafeNodeAPI.Tests.IntegrationTests
{
    public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact(DisplayName = "Register + Login Flow Should Return Valid JWT + RefreshToken")]
        public async Task RegisterAndLogin_ShouldReturnJwtAndRefreshToken()
        {
            var email = $"testuser_{Guid.NewGuid()}@example.com";
            var password = "StrongPass@123";

            var registerRequest = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = password,
                Role = Models.Constants.UserRole.Viewer
            };

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act - Register
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/registerUser", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            // Act - Login
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // Assert
            loginResult.Should().NotBeNull();
            loginResult!.Token.Should().NotBeNullOrWhiteSpace();
            loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
            loginResult.Expiry.Should().BeAfter(DateTime.UtcNow);
            loginResult.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);

            // Parse JWT and validate claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(loginResult.Token);

            jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Viewer");
        }
    }
}
