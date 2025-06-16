using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using SafeNodeAPI.Models.Constants;

namespace SafeNodeAPI.Src.Services.Azure
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly AzureBlobSettings _settings;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IOptions<AzureBlobSettings> options)
        {
            _settings = options.Value;
            _containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(IFormFile file, string blobName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            try
            {
                var blob = _containerClient.GetBlobClient(blobName);

                await using var stream = file.OpenReadStream();
                var response = await blob.UploadAsync(stream, overwrite: true);

                if (response == null || response.GetRawResponse().Status >= 400)
                    throw new InvalidOperationException("Upload failed with status: " + response?.GetRawResponse().Status);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to upload file to blob storage: {ex.Message}", ex);
            }
        }


        public async Task<Stream> DownloadAsync(string blobName)
        {
            try
            {
                var blob = _containerClient.GetBlobClient(blobName);

                if (!await blob.ExistsAsync())
                    throw new FileNotFoundException("Blob not found: " + blobName);

                var response = await blob.DownloadAsync();
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new FileNotFoundException("Blob not found: " + blobName, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to download blob '{blobName}': {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string blobName)
        {
            try
            {
                var blob = _containerClient.GetBlobClient(blobName);
                return await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to delete blob '{blobName}': {ex.Message}", ex);
            }
        }

    }

}
