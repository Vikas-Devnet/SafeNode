using SafeNodeAPI.Models.Constants;

namespace SafeNodeAPI.Models.Request
{
    public class ProvideAccessRequest
    {
        public int FolderId { get; set; }
        public int TargetUserId { get; set; }
        public required UserRole AccessLevel { get; set; }
    }
}
