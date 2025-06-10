using System.ComponentModel.DataAnnotations;

namespace SafeNodeAPI.Models.Request
{
    public class RefreshTokenRequest
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string RefreshToken { get; set; }
    }
}
