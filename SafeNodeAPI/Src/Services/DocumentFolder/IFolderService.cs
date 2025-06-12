using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;

namespace SafeNodeAPI.Src.Services.DocumentFolder
{
    public interface IFolderService
    {
        Task<FolderResponse> CreateFolderAsync(FolderRequest request, int userId);
    }
}
