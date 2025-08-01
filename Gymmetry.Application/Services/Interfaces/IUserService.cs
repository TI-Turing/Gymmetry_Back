using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.User.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.User.Response;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationResponse<User>> CreateUserAsync(AddRequest request);
        Task<ApplicationResponse<User>> GetUserByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<User>>> GetAllUsersAsync();
        Task<ApplicationResponse<bool>> UpdateUserAsync(UpdateRequest request, string ip="",  string invocationId = "");
        Task<ApplicationResponse<bool>> DeleteUserAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<User>>> FindUsersByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<bool>> UpdatePasswordAsync(PasswordUserRequest request, string invocationId = "");
        Task<ApplicationResponse<bool>> UpdateUserGymAsync(Guid userId, Guid gymId, string ip="",  string invocationId = "", bool owner = false);
        Task<ApplicationResponse<bool>> UpdateUsersGymToNullAsync(Guid gymId, string ip ="", string invocationId = "");
        Task<ValidateUserFieldsResponse> ValidateUserFieldsAsync(Guid userId);
        Task<ApplicationResponse<string>> UploadUserProfileImageAsync(UploadUserProfileImageRequest request);
        Task<ApplicationResponse<bool>> PhoneExistsAsync(string phone);
    }
}
