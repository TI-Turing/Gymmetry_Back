using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyRepository
    {
        Daily CreateDaily(Daily entity);
        Daily GetDailyById(Guid id);
        IEnumerable<Daily> GetAllDailies();
        bool UpdateDaily(Daily entity);
        bool DeleteDaily(Guid id);
        IEnumerable<Daily> FindDailiesByFields(Dictionary<string, object> filters);
    }
}
