namespace SafeNodeAPI.Models.Request
{
    public class RevokeAccessRequest
    {
        public int FolderId { get; set; }
        public int UserId { get; set; }
    }
}
