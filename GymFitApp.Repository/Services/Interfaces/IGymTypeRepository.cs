using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymTypeRepository
    {
        Task<GymType> CreateGymTypeAsync(GymType entity);
        Task<GymType?> GetGymTypeByIdAsync(Guid id);
        Task<IEnumerable<GymType>> GetAllGymTypesAsync();
        Task<bool> UpdateGymTypeAsync(GymType entity);
        Task<bool> DeleteGymTypeAsync(Guid id);
        Task<IEnumerable<GymType>> FindGymTypesByFieldsAsync(Dictionary<string, object> filters);
    }
}
