using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;

namespace SafeNodeAPI.Src.Services.DocumentFolder
{
    public interface IFolderService
    {
        Task<FolderResponse> CreateFolderAsync(FolderRequest request, int userId);
        Task<FolderResponse?> GetSubFolderByID(int folderId, int userId);
        Task<IEnumerable<FolderResponse>> GetRootFoldersByUserIdAsync(int userId);
        Task DeleteFolderById(int folderId, int userId);
        Task ProvideFolderAccessAsync(ProvideAccessRequest request, int requesterUserId);
        Task RevokeFolderAccessAsync(RevokeAccessRequest request, int requesterUserId);
    }
}
