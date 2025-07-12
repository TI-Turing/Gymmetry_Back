using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IEmployeeTypeRepository
    {
        Task<EmployeeType> CreateEmployeeTypeAsync(EmployeeType entity);
        Task<EmployeeType?> GetEmployeeTypeByIdAsync(Guid id);
        Task<IEnumerable<EmployeeType>> GetAllEmployeeTypesAsync();
        Task<bool> UpdateEmployeeTypeAsync(EmployeeType entity);
        Task<bool> DeleteEmployeeTypeAsync(Guid id);
        Task<IEnumerable<EmployeeType>> FindEmployeeTypesByFieldsAsync(Dictionary<string, object> filters);
    }
}
