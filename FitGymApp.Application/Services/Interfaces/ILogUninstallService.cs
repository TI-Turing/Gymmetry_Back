using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.LogUninstall.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface ILogUninstallService
    {
        ApplicationResponse<LogUninstall> CreateLogUninstall(AddLogUninstallRequest request);
        ApplicationResponse<LogUninstall> GetLogUninstallById(Guid id);
        ApplicationResponse<IEnumerable<LogUninstall>> GetAllLogUninstalls();
        ApplicationResponse<bool> UpdateLogUninstall(UpdateLogUninstallRequest request);
        ApplicationResponse<bool> DeleteLogUninstall(Guid id);
        ApplicationResponse<IEnumerable<LogUninstall>> FindLogUninstallsByFields(Dictionary<string, object> filters);
    }
}
