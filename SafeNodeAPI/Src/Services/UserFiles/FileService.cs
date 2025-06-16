
using SafeNodeAPI.Models.Constants;
using SafeNodeAPI.Models.DTO;
using SafeNodeAPI.Models.Request;
using SafeNodeAPI.Models.Response;
using SafeNodeAPI.Src.Repository.UserFiles;
using SafeNodeAPI.Src.Services.Azure;
using SafeNodeAPI.Src.Services.Permissions;

namespace SafeNodeAPI.Src.Services.UserFiles
{
    public class FileService(IFileRepository _fileRepo, IAzureBlobStorageService _blobStorageService,
        IPermissionService _permissionService) : IFileService
    {
        public async Task<(Stream stream, string contentType, string fileName)> DownloadFileAsync(int fileId, int userId)
        {
            var record = await _fileRepo.GetFileByIdAsync(fileId);
            if (record == null || record.IsDeleted)
                throw new FileNotFoundException("File not found or has been deleted.");

            var accessLevel = await _permissionService.GetUserFileAccessLevelAsync(userId, fileId);
            if (accessLevel == null || accessLevel is not UserRole.Admin and not UserRole.Editor and not UserRole.Viewer)
                throw new UnauthorizedAccessException("You do not have permission to download this file.");

            var stream = await _blobStorageService.DownloadAsync(record.BlobStorageName);

            return (stream, record.ContentType, record.FileName);
        }


        public async Task<UploadFileResponse> UploadFileAsync(UploadFileRequest request, int userId)
        {
            var file = request.File;
            if (request.File == null || request.File.Length == 0)
                throw new ArgumentException("Uploaded file is empty or missing.");

            if (request.FolderId.HasValue && request.FolderId > 0)
            {
                var accessLevel = await _permissionService.GetUserFolderAccessLevelAsync(userId, request.FolderId.Value);

                if (accessLevel is not UserRole.Admin and not UserRole.Editor)
                    throw new UnauthorizedAccessException("You do not have permission to upload to this folder.");
            }
            var blobName = $"user-{userId}/{Guid.NewGuid()}_{file.FileName}";

            var uploadResponse = await _blobStorageService.UploadAsync(file, blobName);

            var record = new FileRecord
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                BlobStorageName = blobName,
                CreatedByUserId = userId,
                FolderId = request.FolderId,
                UploadedAt = DateTime.UtcNow
            };

            await _fileRepo.CreateFileAsync(record);
            UploadFileResponse response = new()
            {
                FileId = record.Id,
                FileName = record.FileName,
                BlobUrl = uploadResponse,
                ContentType = record.ContentType,
                FileSize = record.FileSize,
                UploadedAt = record.UploadedAt,
                Message = "File uploaded successfully."
            };
            return response;
        }

        public async Task<FileDeleteResponse?> DeleteFileAsync(int fileId, int userId)
        {
            var record = await _fileRepo.GetFileByIdAsync(fileId);
            if (record == null || record.IsDeleted)
                throw new FileNotFoundException("File not found or has been deleted.");

            var accessLevel = await _permissionService.GetUserFileAccessLevelAsync(userId, fileId);
            if (accessLevel == null || accessLevel != UserRole.Admin)
                throw new UnauthorizedAccessException("You do not have permission to delete this file.");

            record.IsDeleted = true;
            record.DeletedAt = DateTime.UtcNow;
            try
            {
                await _blobStorageService.DeleteAsync(record.BlobStorageName);
            }
            catch (FileNotFoundException)
            {
                // If the blob does not exist, we can still mark the file as deleted
                // Log this if necessary
            }
            var fileDetails = await _fileRepo.UpdateFileAsync(record);
            return new FileDeleteResponse
            {
                FileId = fileDetails?.Id ?? 0,
                IsDeleted = true,
                DeletedAt = fileDetails?.DeletedAt ?? DateTime.UtcNow,
                Message = "File deleted successfully."
            };
        }
    }
}
