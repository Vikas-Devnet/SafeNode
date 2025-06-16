namespace SafeNodeAPI.Models.Request
{
    public class UploadFileRequest
    {
        public required IFormFile File { get; set; }
        public int? FolderId { get; set; }
    }
}
