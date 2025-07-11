using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogErrorRepository
    {
        LogError CreateLogError(LogError entity);
        LogError GetLogErrorById(Guid id);
        IEnumerable<LogError> GetAllLogErrors();
        bool UpdateLogError(LogError entity);
        bool DeleteLogError(Guid id);
        IEnumerable<LogError> FindLogErrorsByFields(Dictionary<string, object> filters);
    }
}
