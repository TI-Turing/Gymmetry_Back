using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyRepository
    {
        Task<Daily> CreateDailyAsync(Daily entity);
        Task<Daily?> GetDailyByIdAsync(Guid id);
        Task<IEnumerable<Daily>> GetAllDailiesAsync();
        Task<bool> UpdateDailyAsync(Daily entity);
        Task<bool> DeleteDailyAsync(Guid id);
        Task<IEnumerable<Daily>> FindDailiesByFieldsAsync(Dictionary<string, object> filters);
    }
}
