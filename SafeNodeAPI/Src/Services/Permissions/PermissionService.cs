using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.Constants;

namespace SafeNodeAPI.Src.Services.Permissions
{
    public class PermissionService(SafeNodeDbContext _context) : IPermissionService
    {
        public async Task<UserRole?> GetUserFolderAccessLevelAsync(int userId, int? folderId)
        {
            while (true)
            {
                var permission = await _context.FolderPermissions.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.FolderId == folderId);

                if (permission != null)
                    return permission.AccessLevel;

                var parentId = await _context.Folders.AsNoTracking()
                    .Where(f => f.Id == folderId)
                    .Select(f => f.ParentFolderId)
                    .FirstOrDefaultAsync();

                if (parentId == null)
                    return null;

                folderId = parentId.Value;
            }
        }
    }
}
