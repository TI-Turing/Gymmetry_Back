using System.Threading.Tasks;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IAuthRepository
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}