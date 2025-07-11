using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.EmployeeUser.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IEmployeeUserService
    {
        ApplicationResponse<EmployeeUser> CreateEmployeeUser(AddEmployeeUserRequest request);
        ApplicationResponse<EmployeeUser> GetEmployeeUserById(Guid id);
        ApplicationResponse<IEnumerable<EmployeeUser>> GetAllEmployeeUsers();
        ApplicationResponse<bool> UpdateEmployeeUser(UpdateEmployeeUserRequest request);
        ApplicationResponse<bool> DeleteEmployeeUser(Guid id);
        ApplicationResponse<IEnumerable<EmployeeUser>> FindEmployeeUsersByFields(Dictionary<string, object> filters);
    }
}
