using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.UserType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IUserTypeService
    {
        ApplicationResponse<UserType> CreateUserType(AddUserTypeRequest request);
        ApplicationResponse<UserType> GetUserTypeById(Guid id);
        ApplicationResponse<IEnumerable<UserType>> GetAllUserTypes();
        ApplicationResponse<bool> UpdateUserType(UpdateUserTypeRequest request);
        ApplicationResponse<bool> DeleteUserType(Guid id);
        ApplicationResponse<IEnumerable<UserType>> FindUserTypesByFields(Dictionary<string, object> filters);
    }
}
