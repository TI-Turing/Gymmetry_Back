using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ISubModuleRepository
    {
        SubModule CreateSubModule(SubModule entity);
        SubModule GetSubModuleById(Guid id);
        IEnumerable<SubModule> GetAllSubModules();
        bool UpdateSubModule(SubModule entity);
        bool DeleteSubModule(Guid id);
        IEnumerable<SubModule> FindSubModulesByFields(Dictionary<string, object> filters);
    }
}
