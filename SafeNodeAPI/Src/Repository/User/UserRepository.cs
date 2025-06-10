using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.User
{
    public class UserRepository(SafeNodeDbContext _dbContext) : IUserRepository
    {
        public Task<UserMaster?> GetUserByEmailAsync(string email)
        {
            return _dbContext.UserMaster
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserMaster> CreateUserAsync(UserMaster user)
        {
            _dbContext.UserMaster.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while saving the user.", ex);
            }
            return user;
        }

        public async Task<UserMaster?> UpdateUserAsync(UserMaster user)
        {
            _dbContext.UserMaster.Update(user);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while updating the user.", ex);
            }
            return user;
        }
    }
}
