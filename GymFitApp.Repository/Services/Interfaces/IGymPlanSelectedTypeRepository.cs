using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymPlanSelectedTypeRepository
    {
        GymPlanSelectedType CreateGymPlanSelectedType(GymPlanSelectedType entity);
        GymPlanSelectedType GetGymPlanSelectedTypeById(Guid id);
        IEnumerable<GymPlanSelectedType> GetAllGymPlanSelectedTypes();
        bool UpdateGymPlanSelectedType(GymPlanSelectedType entity);
        bool DeleteGymPlanSelectedType(Guid id);
        IEnumerable<GymPlanSelectedType> FindGymPlanSelectedTypesByFields(Dictionary<string, object> filters);
    }
}
