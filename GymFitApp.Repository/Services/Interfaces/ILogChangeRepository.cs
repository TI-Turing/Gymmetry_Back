using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogChangeRepository
    {
        LogChange CreateLogChange(LogChange entity);
        LogChange GetLogChangeById(Guid id);
        IEnumerable<LogChange> GetAllLogChanges();
        bool UpdateLogChange(LogChange entity);
        bool DeleteLogChange(Guid id);
        IEnumerable<LogChange> FindLogChangesByFields(Dictionary<string, object> filters);
    }
}
