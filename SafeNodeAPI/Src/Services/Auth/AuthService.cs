using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SafeNodeAPI.Models.Constants;
using SafeNodeAPI.Models.DTO;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;
using SafeNodeAPI.Src.Repository.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SafeNodeAPI.Src.Services.Auth
{
    public class AuthService(IUserRepository userRepo, IOptions<JwtSettings> options) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly JwtSettings _jwtSettings = options.Value;
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Invalid email or password");

            var token = CreateJwtToken(user, out DateTime expiryDate);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await _userRepo.UpdateUserAsync(user);
            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiry = expiryDate,
                RefreshTokenExpiry = user.RefreshTokenExpiry,
                Message = "Logged in Successfully"
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User already exists.");

            CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

            var user = new UserMaster
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            var savedUser = await _userRepo.CreateUserAsync(user);

            return new RegisterResponse
            {
                UserId = savedUser.Id,
                Email = savedUser.Email,
                Message = "User Registered Successfully"
            };
        }
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var newJwtToken = CreateJwtToken(user, out DateTime expiry);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await _userRepo.UpdateUserAsync(user);

            return new RefreshTokenResponse
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                Expiry = expiry,
                RefreshTokenExpiry = user.RefreshTokenExpiry,
                Message = "Token refreshed successfully"
            };
        }


        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
        private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
        private string CreateJwtToken(UserMaster user, out DateTime expiryDate)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            expiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryDate,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

    }

}
