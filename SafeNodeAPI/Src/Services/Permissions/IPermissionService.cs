using SafeNodeAPI.Models.Constants;

namespace SafeNodeAPI.Src.Services.Permissions
{
    public interface IPermissionService
    {
        Task<UserRole?> GetUserFolderAccessLevelAsync(int userId, int? folderId);
    }
}
