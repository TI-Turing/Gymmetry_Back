using System.Security.Claims;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Auth.Request;
using Gymmetry.Domain.DTO.Auth.Response;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IClientCredentialsService
    {
        Task<ApplicationResponse<ClientCredentialsResponse>> GetTokenAsync(ClientCredentialsRequest request, string? ip = null);
        ClaimsIdentity BuildIdentityFromClient(string clientId, string[]? scopes);
    }
}
