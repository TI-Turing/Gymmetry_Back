using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Schedule.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IScheduleService
    {
        ApplicationResponse<Schedule> CreateSchedule(AddScheduleRequest request);
        ApplicationResponse<Schedule> GetScheduleById(Guid id);
        ApplicationResponse<IEnumerable<Schedule>> GetAllSchedules();
        ApplicationResponse<bool> UpdateSchedule(UpdateScheduleRequest request);
        ApplicationResponse<bool> DeleteSchedule(Guid id);
        ApplicationResponse<IEnumerable<Schedule>> FindSchedulesByFields(Dictionary<string, object> filters);
    }
}
