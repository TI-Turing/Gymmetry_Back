using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogLoginRepository
    {
        LogLogin CreateLogLogin(LogLogin entity);
        LogLogin GetLogLoginById(Guid id);
        IEnumerable<LogLogin> GetAllLogLogins();
        bool UpdateLogLogin(LogLogin entity);
        bool DeleteLogLogin(Guid id);
        IEnumerable<LogLogin> FindLogLoginsByFields(Dictionary<string, object> filters);
    }
}
