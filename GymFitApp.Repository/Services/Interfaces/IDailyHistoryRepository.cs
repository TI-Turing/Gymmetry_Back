using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyHistoryRepository
    {
        DailyHistory CreateDailyHistory(DailyHistory entity);
        DailyHistory GetDailyHistoryById(Guid id);
        IEnumerable<DailyHistory> GetAllDailyHistories();
        bool UpdateDailyHistory(DailyHistory entity);
        bool DeleteDailyHistory(Guid id);
        IEnumerable<DailyHistory> FindDailyHistoriesByFields(Dictionary<string, object> filters);
    }
}
