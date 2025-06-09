using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeNodeAPI.Models.DTO
{
    [Table("Folder")]
    [Index(nameof(FolderName))]
    public class Folder
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public required string FolderName { get; set; }

        public int? ParentFolderId { get; set; }

        [ForeignKey(nameof(ParentFolderId))]
        public Folder? ParentFolder { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public required UserMaster User { get; set; }

        public ICollection<Folder>? SubFolders { get; set; }
        public ICollection<FileRecord>? Files { get; set; }
    }
}
