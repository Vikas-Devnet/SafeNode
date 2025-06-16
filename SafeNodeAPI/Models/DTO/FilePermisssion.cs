using SafeNodeAPI.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeNodeAPI.Models.DTO
{
    [Table("FilePermission")]
    public class FilePermission
    {
        [Key]
        public int Id { get; set; }

        public int FileId { get; set; }
        [ForeignKey(nameof(FileId))]
        public FileRecord? File { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public UserMaster? User { get; set; }

        public UserRole AccessLevel { get; set; } = UserRole.Viewer;
    }
}
