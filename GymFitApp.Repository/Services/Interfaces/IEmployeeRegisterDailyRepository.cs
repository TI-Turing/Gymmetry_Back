using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IEmployeeRegisterDailyRepository
    {
        EmployeeRegisterDaily CreateEmployeeRegisterDaily(EmployeeRegisterDaily entity);
        EmployeeRegisterDaily GetEmployeeRegisterDailyById(Guid id);
        IEnumerable<EmployeeRegisterDaily> GetAllEmployeeRegisterDailies();
        bool UpdateEmployeeRegisterDaily(EmployeeRegisterDaily entity);
        bool DeleteEmployeeRegisterDaily(Guid id);
        IEnumerable<EmployeeRegisterDaily> FindEmployeeRegisterDailiesByFields(Dictionary<string, object> filters);
    }
}
