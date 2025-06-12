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
    }
}
