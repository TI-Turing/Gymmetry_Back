using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelectedModule.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymPlanSelectedModuleService
    {
        ApplicationResponse<GymPlanSelectedModule> CreateGymPlanSelectedModule(AddGymPlanSelectedModuleRequest request);
        ApplicationResponse<GymPlanSelectedModule> GetGymPlanSelectedModuleById(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelectedModule>> GetAllGymPlanSelectedModules();
        ApplicationResponse<bool> UpdateGymPlanSelectedModule(UpdateGymPlanSelectedModuleRequest request);
        ApplicationResponse<bool> DeleteGymPlanSelectedModule(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelectedModule>> FindGymPlanSelectedModulesByFields(Dictionary<string, object> filters);
    }
}
