using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IAuthService
    {
        LoginResponse? Login(LoginRequest request);
        RefreshTokenResponse? RefreshToken(RefreshTokenRequest request);
    }
}