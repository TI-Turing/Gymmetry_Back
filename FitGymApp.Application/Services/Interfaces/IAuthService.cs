using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using System.Threading.Tasks;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    }
}