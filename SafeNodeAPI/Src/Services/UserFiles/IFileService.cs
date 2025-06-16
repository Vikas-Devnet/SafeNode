using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;

namespace SafeNodeAPI.Src.Services.UserFiles
{
    public interface IFileService
    {
        Task<UploadFileResponse> UploadFileAsync(UploadFileRequest request, int userId);
        Task<(Stream stream, string contentType, string fileName)> DownloadFileAsync(int fileId, int userId);
        Task<FileDeleteResponse?> DeleteFileAsync(int fileId, int userId);
    }
}
