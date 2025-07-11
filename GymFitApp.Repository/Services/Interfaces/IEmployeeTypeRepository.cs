using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IEmployeeTypeRepository
    {
        EmployeeType CreateEmployeeType(EmployeeType entity);
        EmployeeType GetEmployeeTypeById(Guid id);
        IEnumerable<EmployeeType> GetAllEmployeeTypes();
        bool UpdateEmployeeType(EmployeeType entity);
        bool DeleteEmployeeType(Guid id);
        IEnumerable<EmployeeType> FindEmployeeTypesByFields(Dictionary<string, object> filters);
    }
}
