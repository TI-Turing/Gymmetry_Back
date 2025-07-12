using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPasswordService
    {
        Task<ApplicationResponse<string>> HashPasswordAsync(string password);
        Task<ApplicationResponse<bool>> VerifyPasswordAsync(string password, string hashedPassword);
    }
}
