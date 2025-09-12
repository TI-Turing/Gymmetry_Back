namespace Gymmetry.Domain.Options
{
    public class OAuthClient
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string[] Scopes { get; set; } = [];
        public int TokenExpirationMinutes { get; set; } = 60;
    }

    public class OAuthOptions
    {
        public OAuthClient[] Clients { get; set; } = [];
    }
}
