namespace Gymmetry.Domain.DTO.Auth.Request
{
    public class ClientCredentialsRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? Scope { get; set; }
    }
}
