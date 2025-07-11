using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IJourneyEmployeeRepository
    {
        JourneyEmployee CreateJourneyEmployee(JourneyEmployee entity);
        JourneyEmployee GetJourneyEmployeeById(Guid id);
        IEnumerable<JourneyEmployee> GetAllJourneyEmployees();
        bool UpdateJourneyEmployee(JourneyEmployee entity);
        bool DeleteJourneyEmployee(Guid id);
        IEnumerable<JourneyEmployee> FindJourneyEmployeesByFields(Dictionary<string, object> filters);
    }
}
