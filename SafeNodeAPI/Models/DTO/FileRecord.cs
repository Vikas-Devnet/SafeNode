using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeNodeAPI.Models.DTO
{
    [Table("FileRecord")]
    [Index(nameof(FileName))]
    [Index(nameof(BlobStorageName), IsUnique = true)]
    public class FileRecord
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "varchar(150)")]
        [MaxLength(150)]
        public required string FileName { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public required string ContentType { get; set; }

        public required long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "varchar(350)")]
        [MaxLength(150)]
        public required string BlobStorageName { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public int CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public UserMaster? User { get; set; }

        public int? FolderId { get; set; }

        [ForeignKey(nameof(FolderId))]
        public Folder? Folder { get; set; }
        public ICollection<FilePermission>? FilePermissions { get; set; }

    }
}
