using SafeNodeAPI.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace SafeNodeAPI.Models.Request
{
    public class RegisterRequest
    {
        [MaxLength(50)]
        public required string FirstName { get; set; }
        [MaxLength(50)]
        public string? LastName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
        ErrorMessage = "Password must include uppercase, lowercase, number, and special character.")]
        public required string Password { get; set; }
        [EnumDataType(typeof(UserRole))]
        public UserRole? Role { get; set; } = UserRole.Viewer;
    }
}
