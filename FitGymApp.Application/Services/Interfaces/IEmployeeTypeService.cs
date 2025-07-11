using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.EmployeeType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IEmployeeTypeService
    {
        ApplicationResponse<EmployeeType> CreateEmployeeType(AddEmployeeTypeRequest request);
        ApplicationResponse<EmployeeType> GetEmployeeTypeById(Guid id);
        ApplicationResponse<IEnumerable<EmployeeType>> GetAllEmployeeTypes();
        ApplicationResponse<bool> UpdateEmployeeType(UpdateEmployeeTypeRequest request);
        ApplicationResponse<bool> DeleteEmployeeType(Guid id);
        ApplicationResponse<IEnumerable<EmployeeType>> FindEmployeeTypesByFields(Dictionary<string, object> filters);
    }
}
