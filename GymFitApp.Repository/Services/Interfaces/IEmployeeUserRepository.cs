using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IEmployeeUserRepository
    {
        EmployeeUser CreateEmployeeUser(EmployeeUser entity);
        EmployeeUser GetEmployeeUserById(Guid id);
        IEnumerable<EmployeeUser> GetAllEmployeeUsers();
        bool UpdateEmployeeUser(EmployeeUser entity);
        bool DeleteEmployeeUser(Guid id);
        IEnumerable<EmployeeUser> FindEmployeeUsersByFields(Dictionary<string, object> filters);
    }
}
