using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IScheduleRepository
    {
        Schedule CreateSchedule(Schedule entity);
        Schedule GetScheduleById(Guid id);
        IEnumerable<Schedule> GetAllSchedules();
        bool UpdateSchedule(Schedule entity);
        bool DeleteSchedule(Guid id);
        IEnumerable<Schedule> FindSchedulesByFields(Dictionary<string, object> filters);
    }
}
