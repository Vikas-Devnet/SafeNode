using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.User
{
    public interface IUserRepository
    {
        Task<UserMaster?> GetUserByEmailAsync(string email);
        Task<UserMaster> CreateUserAsync(UserMaster request);
    }
}
