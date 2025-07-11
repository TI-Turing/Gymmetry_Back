using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyHistory.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IDailyHistoryService
    {
        ApplicationResponse<DailyHistory> CreateDailyHistory(AddDailyHistoryRequest request);
        ApplicationResponse<DailyHistory> GetDailyHistoryById(Guid id);
        ApplicationResponse<IEnumerable<DailyHistory>> GetAllDailyHistories();
        ApplicationResponse<bool> UpdateDailyHistory(UpdateDailyHistoryRequest request);
        ApplicationResponse<bool> DeleteDailyHistory(Guid id);
        ApplicationResponse<IEnumerable<DailyHistory>> FindDailyHistoriesByFields(Dictionary<string, object> filters);
    }
}
