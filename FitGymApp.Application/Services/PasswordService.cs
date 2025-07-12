using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using BCrypt.Net;

namespace FitGymApp.Application.Services
{
    public class PasswordService : IPasswordService
    {
        public async Task<ApplicationResponse<string>> HashPasswordAsync(string password)
        {
            try
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(password);
                return new ApplicationResponse<string>
                {
                    Success = true,
                    Data = hash
                };
            }
            catch (System.Exception ex)
            {
                return new ApplicationResponse<string>
                {
                    Success = false,
                    Message = "Error al hashear el password.",
                    ErrorCode = "HashError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> VerifyPasswordAsync(string password, string hashedPassword)
        {
            try
            {
                var result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                return new ApplicationResponse<bool>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (System.Exception ex)
            {
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Message = "Error al verificar el password.",
                    ErrorCode = "VerifyError"
                };
            }
        }
    }
}
