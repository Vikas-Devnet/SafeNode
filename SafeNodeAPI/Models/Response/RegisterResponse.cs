namespace SafeNodeAPI.Models.Response
{
    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
    }
}
