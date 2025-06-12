using SafeNodeAPI.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeNodeAPI.Models.DTO
{
    [Table("FolderPermission")]
    public class FolderPermission
    {
        [Key]
        public int Id { get; set; }
        public int FolderId { get; set; }
        [ForeignKey(nameof(FolderId))]
        public Folder? Folder { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public UserMaster? User { get; set; }
        public UserRole AccessLevel { get; set; } = UserRole.Viewer;
    }
}
