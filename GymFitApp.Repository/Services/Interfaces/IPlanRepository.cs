using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IPlanRepository
    {
        Plan CreatePlan(Plan plan);
        Plan GetPlanById(Guid id);
        IEnumerable<Plan> GetAllPlans();
        bool UpdatePlan(Plan plan);
        bool DeletePlan(Guid id);
        IEnumerable<Plan> FindPlansByFields(Dictionary<string, object> filters);
    }
}
