using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymPlanSelectedTypeRepository
    {
        Task<GymPlanSelectedType> CreateGymPlanSelectedTypeAsync(GymPlanSelectedType entity);
        Task<GymPlanSelectedType?> GetGymPlanSelectedTypeByIdAsync(Guid id);
        Task<IEnumerable<GymPlanSelectedType>> GetAllGymPlanSelectedTypesAsync();
        Task<bool> UpdateGymPlanSelectedTypeAsync(GymPlanSelectedType entity);
        Task<bool> DeleteGymPlanSelectedTypeAsync(Guid id);
        Task<IEnumerable<GymPlanSelectedType>> FindGymPlanSelectedTypesByFieldsAsync(Dictionary<string, object> filters);
    }
}
