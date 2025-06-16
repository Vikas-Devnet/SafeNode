using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.Constants;
using SafeNodeAPI.Models.DTO;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;
using SafeNodeAPI.Src.Repository.DocumentFolder;
using SafeNodeAPI.Src.Services.Permissions;

namespace SafeNodeAPI.Src.Services.DocumentFolder
{
    public class FolderService(IFolderRepository _folderRepo, IPermissionService _permissionService, SafeNodeDbContext _context) : IFolderService
    {
        public async Task<FolderResponse> CreateFolderAsync(FolderRequest request, int userId)
        {
            if (request.ParentFolderId > 0)
            {
                var accessLevel = await _permissionService.GetUserFolderAccessLevelAsync(userId, request.ParentFolderId);
                if (accessLevel is null || accessLevel is not (UserRole.Admin or UserRole.Editor))
                    throw new UnauthorizedAccessException("Insufficient permissions to create a folder in this location.");
            }

            var folder = new Folder
            {
                FolderName = request.FolderName,
                ParentFolderId = request.ParentFolderId == 0 ? null : request.ParentFolderId,
                CreatedByUserId = userId
            };

            var createdFolder = await _folderRepo.AddFolderAsync(folder);

            _context.FolderPermissions.Add(new FolderPermission
            {
                FolderId = createdFolder.Id,
                UserId = userId,
                AccessLevel = UserRole.Admin
            });

            await _context.SaveChangesAsync();

            return new FolderResponse
            {
                Id = createdFolder.Id,
                FolderName = createdFolder.FolderName,
                ParentFolderId = createdFolder.ParentFolderId
            };

        }
        public async Task<FolderResponse?> GetSubFolderByID(int folderId, int userId)
        {
            _ = await _permissionService.GetUserFolderAccessLevelAsync(userId, folderId)
                ?? throw new UnauthorizedAccessException("Insufficient permissions to view a folder in this location.");

            var folder = await _folderRepo.GetFolderByIdAsync(folderId)
                ?? throw new KeyNotFoundException("Folder not found.");

            return MapToResponse(folder);
        }
        public async Task<IEnumerable<FolderResponse>> GetRootFoldersByUserIdAsync(int userId)
        {
            var folders = await _folderRepo.GetRootFoldersByUserIdAsync(userId);
            return [.. folders
                .Where(f => f != null)
                .Select(f => MapToResponse(f!))];
        }
        public async Task DeleteFolderById(int folderId, int userId)
        {
            var folder = await _folderRepo.GetFolderByIdAsync(folderId)
                ?? throw new KeyNotFoundException("Folder not found.");
            var accessLevel = await _permissionService.GetUserFolderAccessLevelAsync(userId, folderId);
            if (accessLevel is null || accessLevel != UserRole.Admin)
                throw new UnauthorizedAccessException("Insufficient permissions to delete this folder.");

            await DeleteFolderRecursive(folder, userId);
            await _context.SaveChangesAsync();
        }
        public async Task ProvideFolderAccessAsync(ProvideAccessRequest request, int requesterUserId)
        {
            var folder = await _folderRepo.GetFolderByIdAsync(request.FolderId)
                ?? throw new KeyNotFoundException("Folder not found.");

            var requesterRole = await _permissionService.GetUserFolderAccessLevelAsync(requesterUserId, folder.Id);
            if (requesterRole is not UserRole.Admin)
                throw new UnauthorizedAccessException("Only Admins can provide access to folders.");

            var existingPermission = await _context.FolderPermissions
                .FirstOrDefaultAsync(p => p.FolderId == folder.Id && p.UserId == request.TargetUserId);

            if (existingPermission != null)
            {
                existingPermission.AccessLevel = request.AccessLevel;
            }
            else
            {
                _context.FolderPermissions.Add(new FolderPermission
                {
                    FolderId = folder.Id,
                    UserId = request.TargetUserId,
                    AccessLevel = request.AccessLevel
                });
            }

            await _context.SaveChangesAsync();
        }
        public async Task RevokeFolderAccessAsync(RevokeAccessRequest request, int requesterUserId)
        {
            var rootFolder = await _folderRepo.GetFolderByIdAsync(request.FolderId)
                ?? throw new KeyNotFoundException("Folder not found.");

            var requesterAccess = await _permissionService.GetUserFolderAccessLevelAsync(requesterUserId, request.FolderId);
            if (requesterAccess is null || requesterAccess != UserRole.Admin)
                throw new UnauthorizedAccessException("You don't have permission to revoke access.");

            var permission = await _context.FolderPermissions
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.FolderId == request.FolderId)
                ?? throw new InvalidOperationException("The user does not have explicit permission on this folder.");

            _context.FolderPermissions.Remove(permission);

            await DetachUserSubfoldersRecursive(rootFolder, request.UserId);

            await _context.SaveChangesAsync();
        }

        private async Task DeleteFolderRecursive(Folder folder, int userId)
        {
            _context.Entry(folder).Collection(f => f.SubFolders!).Load();

            foreach (var subFolder in folder.SubFolders!)
            {
                if (subFolder.CreatedByUserId == userId)
                {
                    await DeleteFolderRecursive(subFolder, userId);
                }
                else
                {
                    subFolder.ParentFolderId = null;
                    _context.Folders.Update(subFolder);
                }
            }

            _context.Folders.Remove(folder);
        }
        private async Task DetachUserSubfoldersRecursive(Folder folder, int revokedUserId)
        {
            _context.Entry(folder).Collection(f => f.SubFolders!).Load();

            foreach (var subFolder in folder.SubFolders!)
            {
                if (subFolder.CreatedByUserId == revokedUserId)
                {
                    subFolder.ParentFolderId = null;
                }
                else
                {
                    await DetachUserSubfoldersRecursive(subFolder, revokedUserId);
                }
            }
        }

        private FolderResponse MapToResponse(Folder folder)
        {
            return new FolderResponse
            {
                Id = folder.Id,
                FolderName = folder.FolderName ?? string.Empty,
                ParentFolderId = folder.ParentFolderId,
                SubFolders = folder.SubFolders?.Select(MapToResponse).ToList()
            };

        }


    }
}
