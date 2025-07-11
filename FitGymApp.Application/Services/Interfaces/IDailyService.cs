using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Daily.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IDailyService
    {
        ApplicationResponse<Daily> CreateDaily(AddDailyRequest request);
        ApplicationResponse<Daily> GetDailyById(Guid id);
        ApplicationResponse<IEnumerable<Daily>> GetAllDailies();
        ApplicationResponse<bool> UpdateDaily(UpdateDailyRequest request);
        ApplicationResponse<bool> DeleteDaily(Guid id);
        ApplicationResponse<IEnumerable<Daily>> FindDailiesByFields(Dictionary<string, object> filters);
    }
}
