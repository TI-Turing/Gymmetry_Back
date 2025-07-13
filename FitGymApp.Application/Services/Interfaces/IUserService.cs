using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.User.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationResponse<User>> CreateUserAsync(AddRequest request);
        Task<ApplicationResponse<User>> GetUserByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<User>>> GetAllUsersAsync();
        Task<ApplicationResponse<bool>> UpdateUserAsync(UpdateRequest request);
        Task<ApplicationResponse<bool>> DeleteUserAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<User>>> FindUsersByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<bool>> UpdatePasswordAsync(PasswordUserRequest request);
    }
}
