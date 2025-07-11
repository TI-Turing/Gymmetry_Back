using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.JourneyEmployee.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IJourneyEmployeeService
    {
        ApplicationResponse<JourneyEmployee> CreateJourneyEmployee(AddJourneyEmployeeRequest request);
        ApplicationResponse<JourneyEmployee> GetJourneyEmployeeById(Guid id);
        ApplicationResponse<IEnumerable<JourneyEmployee>> GetAllJourneyEmployees();
        ApplicationResponse<bool> UpdateJourneyEmployee(UpdateJourneyEmployeeRequest request);
        ApplicationResponse<bool> DeleteJourneyEmployee(Guid id);
        ApplicationResponse<IEnumerable<JourneyEmployee>> FindJourneyEmployeesByFields(Dictionary<string, object> filters);
    }
}
