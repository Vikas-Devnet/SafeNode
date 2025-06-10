using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeNodeAPI.Models.DTO
{
    [Table("UserMaster")]
    [Index(nameof(Email), IsUnique = true)]
    public class UserMaster
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Column(TypeName = "varchar(350)")]
        [MaxLength(350)]
        [EmailAddress]
        public required string Email { get; set; }
        public UserRole? Role { get; set; } = UserRole.Viewer;
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        [Column(TypeName = "varchar(1000)")]
        [MaxLength(1000)]
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<FileRecord>? Files { get; set; }
        public ICollection<Folder>? Folders { get; set; }
    }
}
