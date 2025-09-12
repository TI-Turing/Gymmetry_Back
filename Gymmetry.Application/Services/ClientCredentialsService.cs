using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Auth.Request;
using Gymmetry.Domain.DTO.Auth.Response;
using Gymmetry.Domain.Options;
using Microsoft.Extensions.Options;

namespace Gymmetry.Application.Services
{
    public class ClientCredentialsService : IClientCredentialsService
    {
        private readonly OAuthOptions _options;
        public ClientCredentialsService(IOptions<OAuthOptions> options)
        {
            _options = options.Value ?? new OAuthOptions();
        }

        public ClaimsIdentity BuildIdentityFromClient(string clientId, string[]? scopes)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim("client_id", clientId),
                new Claim("auth_type", "client_credentials"),
            }.ToList();
            if (scopes != null && scopes.Length > 0)
            {
                claims.Add(new Claim("scope", string.Join(' ', scopes)));
            }
            return new ClaimsIdentity(claims);
        }

        public async Task<ApplicationResponse<ClientCredentialsResponse>> GetTokenAsync(ClientCredentialsRequest request, string? ip = null)
        {
            var client = _options.Clients.FirstOrDefault(c => c.ClientId == request.ClientId && c.ClientSecret == request.ClientSecret);
            if (client == null)
            {
                return ApplicationResponse<ClientCredentialsResponse>.ErrorResponse("Invalid client credentials.", "Unauthorized");
            }

            var scopes = (request.Scope ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (scopes.Length > 0 && client.Scopes?.Length > 0)
            {
                if (!scopes.All(s => client.Scopes.Contains(s)))
                {
                    return ApplicationResponse<ClientCredentialsResponse>.ErrorResponse("Invalid scope.", "InvalidScope");
                }
            }

            var identity = BuildIdentityFromClient(client.ClientId, scopes.Length > 0 ? scopes : client.Scopes);
            // Emitir token reutilizando generador existente. Sub será un GUID aleatorio para cumplir con validaciones actuales.
            var token = await JwtTokenGenerator.GenerateTokenAsync(Guid.NewGuid(), client.Name ?? client.ClientId, $"{client.ClientId}@clients", client.TokenExpirationMinutes);
            var data = new ClientCredentialsResponse
            {
                AccessToken = token,
                ExpiresIn = client.TokenExpirationMinutes * 60,
                ExpirationUtc = DateTime.UtcNow.AddMinutes(client.TokenExpirationMinutes),
                Scope = scopes.Length > 0 ? string.Join(' ', scopes) : (client.Scopes?.Length > 0 ? string.Join(' ', client.Scopes) : null),
                ClientId = client.ClientId
            };
            return ApplicationResponse<ClientCredentialsResponse>.SuccessResponse(data, "Token issued.");
        }
    }
}
