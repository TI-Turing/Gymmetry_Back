using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.SubModule.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface ISubModuleService
    {
        ApplicationResponse<SubModule> CreateSubModule(AddSubModuleRequest request);
        ApplicationResponse<SubModule> GetSubModuleById(Guid id);
        ApplicationResponse<IEnumerable<SubModule>> GetAllSubModules();
        ApplicationResponse<bool> UpdateSubModule(UpdateSubModuleRequest request);
        ApplicationResponse<bool> DeleteSubModule(Guid id);
        ApplicationResponse<IEnumerable<SubModule>> FindSubModulesByFields(Dictionary<string, object> filters);
    }
}
