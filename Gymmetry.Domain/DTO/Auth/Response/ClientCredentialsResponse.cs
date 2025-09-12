using System;

namespace Gymmetry.Domain.DTO.Auth.Response
{
    public class ClientCredentialsResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime ExpirationUtc { get; set; }
        public string? Scope { get; set; }
        public string? ClientId { get; set; }
    }
}
