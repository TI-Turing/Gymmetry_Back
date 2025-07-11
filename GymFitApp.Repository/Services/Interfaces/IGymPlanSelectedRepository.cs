using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymPlanSelectedRepository
    {
        GymPlanSelected CreateGymPlanSelected(GymPlanSelected entity);
        GymPlanSelected GetGymPlanSelectedById(Guid id);
        IEnumerable<GymPlanSelected> GetAllGymPlanSelecteds();
        bool UpdateGymPlanSelected(GymPlanSelected entity);
        bool DeleteGymPlanSelected(Guid id);
        IEnumerable<GymPlanSelected> FindGymPlanSelectedsByFields(Dictionary<string, object> filters);
    }
}
