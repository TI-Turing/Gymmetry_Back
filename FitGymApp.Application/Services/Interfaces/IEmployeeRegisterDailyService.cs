using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.EmployeeRegisterDaily.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IEmployeeRegisterDailyService
    {
        ApplicationResponse<EmployeeRegisterDaily> CreateEmployeeRegisterDaily(AddEmployeeRegisterDailyRequest request);
        ApplicationResponse<EmployeeRegisterDaily> GetEmployeeRegisterDailyById(Guid id);
        ApplicationResponse<IEnumerable<EmployeeRegisterDaily>> GetAllEmployeeRegisterDailies();
        ApplicationResponse<bool> UpdateEmployeeRegisterDaily(UpdateEmployeeRegisterDailyRequest request);
        ApplicationResponse<bool> DeleteEmployeeRegisterDaily(Guid id);
        ApplicationResponse<IEnumerable<EmployeeRegisterDaily>> FindEmployeeRegisterDailiesByFields(Dictionary<string, object> filters);
    }
}
