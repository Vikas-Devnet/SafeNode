﻿namespace SafeNodeAPI.Models.Response
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string? Message { get; set; }
    }
}
