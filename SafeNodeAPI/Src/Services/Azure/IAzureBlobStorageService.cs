namespace SafeNodeAPI.Src.Services.Azure
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadAsync(IFormFile file, string blobName);
        Task<Stream> DownloadAsync(string blobName);
        Task<bool> DeleteAsync(string blobName);
    }
}
