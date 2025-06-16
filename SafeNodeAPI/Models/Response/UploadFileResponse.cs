
namespace SafeNodeAPI.Models.Response
{
    public class UploadFileResponse
    {
        public int FileId { get; set; }
        public string? FileName { get; set; }
        public string? BlobUrl { get; set; }
        public string? Message { get; set; }
        public long? FileSize { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string? ContentType { get; set; }
    }
}
