using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.User.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IUserService
    {
        ApplicationResponse<User> CreateUser(AddRequest request);
        ApplicationResponse<User> GetUserById(Guid id);
        ApplicationResponse<IEnumerable<User>> GetAllUsers();
        ApplicationResponse<bool> UpdateUser(UpdateRequest request);
        ApplicationResponse<bool> DeleteUser(Guid id);
        ApplicationResponse<IEnumerable<User>> FindUsersByFields(Dictionary<string, object> filters);
    }
}
