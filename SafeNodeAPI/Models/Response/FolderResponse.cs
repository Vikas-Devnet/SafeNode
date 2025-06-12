namespace SafeNodeAPI.Models.Response
{
    public class FolderResponse
    {
        public int Id { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
    }
}
