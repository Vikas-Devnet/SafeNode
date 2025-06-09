using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;

namespace SafeNodeAPI.Src.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
