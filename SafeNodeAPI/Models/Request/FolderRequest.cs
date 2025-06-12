using System.ComponentModel.DataAnnotations;

namespace SafeNodeAPI.Models.Request
{
    public class FolderRequest
    {
        [MaxLength(50)]
        public required string FolderName { get; set; }
        public int? ParentFolderId { get; set; }
    }
}
