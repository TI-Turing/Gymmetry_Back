using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymPlanSelectedModuleRepository
    {
        GymPlanSelectedModule CreateGymPlanSelectedModule(GymPlanSelectedModule entity);
        GymPlanSelectedModule GetGymPlanSelectedModuleById(Guid id);
        IEnumerable<GymPlanSelectedModule> GetAllGymPlanSelectedModules();
        bool UpdateGymPlanSelectedModule(GymPlanSelectedModule entity);
        bool DeleteGymPlanSelectedModule(Guid id);
        IEnumerable<GymPlanSelectedModule> FindGymPlanSelectedModulesByFields(Dictionary<string, object> filters);
    }
}
