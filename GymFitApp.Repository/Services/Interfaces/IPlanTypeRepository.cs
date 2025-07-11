using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IPlanTypeRepository
    {
        PlanType CreatePlanType(PlanType entity);
        PlanType GetPlanTypeById(Guid id);
        IEnumerable<PlanType> GetAllPlanTypes();
        bool UpdatePlanType(PlanType entity);
        bool DeletePlanType(Guid id);
        IEnumerable<PlanType> FindPlanTypesByFields(Dictionary<string, object> filters);
    }
}
