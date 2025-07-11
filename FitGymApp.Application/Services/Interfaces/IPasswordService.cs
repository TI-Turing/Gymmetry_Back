using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPasswordService
    {
        ApplicationResponse<string> HashPassword(string password);
        ApplicationResponse<bool> VerifyPassword(string password, string hashedPassword);
    }
}
