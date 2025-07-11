using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogUninstallRepository
    {
        LogUninstall CreateLogUninstall(LogUninstall entity);
        LogUninstall GetLogUninstallById(Guid id);
        IEnumerable<LogUninstall> GetAllLogUninstalls();
        bool UpdateLogUninstall(LogUninstall entity);
        bool DeleteLogUninstall(Guid id);
        IEnumerable<LogUninstall> FindLogUninstallsByFields(Dictionary<string, object> filters);
    }
}
