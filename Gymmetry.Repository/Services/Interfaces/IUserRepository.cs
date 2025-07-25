using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<IEnumerable<User>> FindUsersByFieldsAsync(Dictionary<string, object> filters);
        Task<bool> BulkUpdateFieldAsync(IEnumerable<Guid> userIds, string fieldName, object? value);
        Task SaveChangesAsync();
        Task<string> UploadUserProfileImageAsync(Guid userId, byte[] image);
        Task<bool> PhoneExistsAsync(string phone);
        Task<bool> SendSmsAsync(string to, string message);
        Task<bool> SendWhatsappAsync(string to, string message);
        Task SaveUserOtpAsync(UserOTP otp);
    }
}
