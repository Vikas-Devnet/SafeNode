using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SafeNodeAPI.Models.Constants;
using SafeNodeAPI.Models.DTO;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Src.Repository.User;
using SafeNodeAPI.Src.Services.Auth;
using System.Security.Cryptography;
using System.Text;

namespace SafeNodeAPI.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _jwtOptions = Options.Create(new JwtSettings
            {
                SecretKey = "ThisIsASecretKeyForJwtTest-123456789",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryMinutes = 60
            });
            _authService = new AuthService(_mockUserRepo.Object, _jwtOptions);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_AndReturnResponse()
        {
            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "password",
                Role = UserRole.Viewer
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync((UserMaster?)null);

            _mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<UserMaster>()))
                         .ReturnsAsync((UserMaster u) => u);

            var response = await _authService.RegisterAsync(request);

            response.Should().NotBeNull();
            response.Email.Should().Be(request.Email);
            response.Role.Should().Be(UserRole.Viewer.ToString());
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            var password = "password";
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var user = new UserMaster
            {
                Id = 1,
                Email = "test@example.com",
                Role = UserRole.Admin,
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            var loginRequest = new LoginRequest
            {
                Email = user.Email,
                Password = password
            };

            var result = await _authService.LoginAsync(loginRequest);

            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUserExists()
        {
            var request = new RegisterRequest
            {
                FirstName = "Test",
                Email = "test@example.com",
                Password = "password",
                Role = UserRole.Admin
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync(CreateMockUser(request.Email, request.Role));

            var act = async () => await _authService.RegisterAsync(request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("User already exists.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync((UserMaster?)null);

            var request = new LoginRequest { Email = "fake@user.com", Password = "wrongpass" };

            var act = async () => await _authService.LoginAsync(request);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
        private static UserMaster CreateMockUser(string email, UserRole? role)
        {
            using var hmac = new HMACSHA512();
            return new UserMaster
            {
                FirstName = "Test",
                Email = email,
                Role = role ?? UserRole.Viewer,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password")),
                PasswordSalt = hmac.Key
            };
        }
    }
}
