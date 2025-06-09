using System.ComponentModel.DataAnnotations;

namespace SafeNodeAPI.Models.Request
{
    public class LoginRequest
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
