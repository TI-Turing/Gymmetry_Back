using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IUninstallOptionRepository
    {
        UninstallOption CreateUninstallOption(UninstallOption entity);
        UninstallOption GetUninstallOptionById(Guid id);
        IEnumerable<UninstallOption> GetAllUninstallOptions();
        bool UpdateUninstallOption(UninstallOption entity);
        bool DeleteUninstallOption(Guid id);
        IEnumerable<UninstallOption> FindUninstallOptionsByFields(Dictionary<string, object> filters);
    }
}
