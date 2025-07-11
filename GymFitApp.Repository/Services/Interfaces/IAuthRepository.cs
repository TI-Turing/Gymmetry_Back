using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IAuthRepository
    {
        LoginResponse? Login(LoginRequest request);
    }
}