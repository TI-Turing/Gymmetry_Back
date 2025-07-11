using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Module.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IModuleService
    {
        ApplicationResponse<Module> CreateModule(AddModuleRequest request);
        ApplicationResponse<Module> GetModuleById(Guid id);
        ApplicationResponse<IEnumerable<Module>> GetAllModules();
        ApplicationResponse<bool> UpdateModule(UpdateModuleRequest request);
        ApplicationResponse<bool> DeleteModule(Guid id);
        ApplicationResponse<IEnumerable<Module>> FindModulesByFields(Dictionary<string, object> filters);
    }
}
