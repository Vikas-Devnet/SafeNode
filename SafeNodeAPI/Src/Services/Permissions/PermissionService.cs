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

        public async Task<UserRole?> GetUserFileAccessLevelAsync(int userId, int fileId)
        {
            var filePermission = await _context.FilePermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FileId == fileId);

            if (filePermission != null)
                return filePermission.AccessLevel;

            var folderId = await _context.FileRecords
                .AsNoTracking()
                .Where(f => f.Id == fileId)
                .Select(f => f.FolderId)
                .FirstOrDefaultAsync();

            if (folderId == null)
                return null;

            return await GetUserFolderAccessLevelAsync(userId, folderId);
        }

    }
}
