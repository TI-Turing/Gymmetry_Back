using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDietRepository
    {
        Task<Diet> CreateDietAsync(Diet diet);
        Task<Diet?> GetDietByIdAsync(Guid id);
        Task<IEnumerable<Diet>> GetAllDietsAsync();
        Task<bool> UpdateDietAsync(Diet diet);
        Task<bool> DeleteDietAsync(Guid id);
        Task<IEnumerable<Diet>> FindDietsByFieldsAsync(Dictionary<string, object> filters);
    }
}
