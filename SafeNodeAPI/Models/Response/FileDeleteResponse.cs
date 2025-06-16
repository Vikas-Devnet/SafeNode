namespace SafeNodeAPI.Models.Response
{
    public class FileDeleteResponse
    {
        public int? FileId { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? Message { get; set; }

    }
}
