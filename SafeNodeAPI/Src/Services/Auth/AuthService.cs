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

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.ToString()??"")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                Expiry = tokenDescriptor.Expires!.Value
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
                Role = request.Role,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            var savedUser = await _userRepo.CreateUserAsync(user);

            return new RegisterResponse
            {
                UserId = savedUser.Id,
                Email = savedUser.Email,
                Role = savedUser.Role.ToString(),
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

    }

}
