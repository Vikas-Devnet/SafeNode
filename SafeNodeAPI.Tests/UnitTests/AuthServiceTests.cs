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
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            });

            _authService = new AuthService(_mockUserRepo.Object, _jwtOptions);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_AndReturnResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "password",
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync((UserMaster?)null);

            _mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<UserMaster>()))
                         .ReturnsAsync((UserMaster u) => u);

            // Act
            var response = await _authService.RegisterAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Email.Should().Be(request.Email);
            response.Message.Should().Be("User Registered Successfully");
            response.UserId.Should().Be(0); // Default value before save
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenUserExists()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "Test",
                Email = "test@example.com",
                Password = "password",
            };

            var existingUser = new UserMaster
            {
                Email = request.Email,
                FirstName = "Existing",
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(request.Email))
                         .ReturnsAsync(existingUser);

            // Act & Assert
            await _authService.Invoking(async x => await x.RegisterAsync(request))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("User already exists.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var password = "password";
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var user = new UserMaster
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = "Test",
                RefreshToken = null,
                RefreshTokenExpiry = null
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            _mockUserRepo.Setup(r => r.UpdateUserAsync(It.IsAny<UserMaster>()))
                         .ReturnsAsync((UserMaster u) => u);

            var loginRequest = new LoginRequest
            {
                Email = user.Email,
                Password = password
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
            result.Expiry.Should().BeAfter(DateTime.UtcNow);
            result.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);
            result.Message.Should().Be("Logged in Successfully");

            _mockUserRepo.Verify(r => r.UpdateUserAsync(It.Is<UserMaster>(u =>
                u.RefreshToken != null &&
                u.RefreshTokenExpiry != null)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync((UserMaster?)null);

            var request = new LoginRequest { Email = "fake@user.com", Password = "wrongpass" };

            // Act & Assert
            await _authService.Invoking(async x => await x.LoginAsync(request))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid email or password");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordDoesNotMatch()
        {
            // Arrange
            var password = "correct";
            var wrongPassword = "wrong";
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var user = new UserMaster
            {
                Email = "test@example.com",
                FirstName = "test",
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            var request = new LoginRequest { Email = user.Email, Password = wrongPassword };

            // Act & Assert
            await _authService.Invoking(async x => await x.LoginAsync(request))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid email or password");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewToken_WhenValid()
        {
            // Arrange
            var oldRefreshToken = "old-refresh-token";
            var user = new UserMaster
            {
                Id = 1,
                Email = "test@example.com",
                FirstName = "Test",
                RefreshToken = oldRefreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10),
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            _mockUserRepo.Setup(r => r.UpdateUserAsync(It.IsAny<UserMaster>()))
                         .ReturnsAsync((UserMaster u) => u);

            var request = new RefreshTokenRequest
            {
                Email = user.Email,
                RefreshToken = oldRefreshToken
            };

            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBe(oldRefreshToken);
            result.Expiry.Should().BeAfter(DateTime.UtcNow);
            result.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);
            result.Message.Should().Be("Token refreshed successfully");

            _mockUserRepo.Verify(r => r.UpdateUserAsync(It.Is<UserMaster>(u =>
                u.RefreshToken != oldRefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow)), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowUnauthorizedAccessException_WhenTokenExpired()
        {
            // Arrange
            var user = new UserMaster
            {
                Email = "expired@test.com",
                FirstName = "Test",
                RefreshToken = "expired-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-1),
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            var request = new RefreshTokenRequest
            {
                Email = user.Email,
                RefreshToken = user.RefreshToken
            };

            // Act & Assert
            await _authService.Invoking(async x => await x.RefreshTokenAsync(request))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid or expired refresh token");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowUnauthorizedAccessException_WhenTokenInvalid()
        {
            // Arrange
            var user = new UserMaster
            {
                Email = "invalid@test.com",
                FirstName = "Test",
                RefreshToken = "actual-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10),
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(user.Email))
                         .ReturnsAsync(user);

            var request = new RefreshTokenRequest
            {
                Email = user.Email,
                RefreshToken = "fake-token"
            };

            // Act & Assert
            await _authService.Invoking(async x => await x.RefreshTokenAsync(request))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid or expired refresh token");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync((UserMaster?)null);

            var request = new RefreshTokenRequest
            {
                Email = "nonexistent@test.com",
                RefreshToken = "any-token"
            };

            // Act & Assert
            await _authService.Invoking(async x => await x.RefreshTokenAsync(request))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid or expired refresh token");
        }
    }
}