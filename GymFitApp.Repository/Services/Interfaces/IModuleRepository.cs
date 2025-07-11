using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IModuleRepository
    {
        Module CreateModule(Module entity);
        Module GetModuleById(Guid id);
        IEnumerable<Module> GetAllModules();
        bool UpdateModule(Module entity);
        bool DeleteModule(Guid id);
        IEnumerable<Module> FindModulesByFields(Dictionary<string, object> filters);
    }
}
