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

        [Fact(DisplayName = "Register + Login Flow Should Return Valid JWT Token")]
        public async Task RegisterAndLogin_ShouldReturnJwtToken()
        {
            var email = $"testuser_{Guid.NewGuid()}@example.com";
            var registerRequest = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                Password = "StrongPass@123",
                Role = Models.Constants.UserRole.Viewer
            };

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = "StrongPass@123"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/registerUser", registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            loginResult.Should().NotBeNull();
            loginResult!.Token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(loginResult.Token);

            token.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
            token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Viewer");
        }
    }
}
