using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.UninstallOption.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IUninstallOptionService
    {
        ApplicationResponse<UninstallOption> CreateUninstallOption(AddUninstallOptionRequest request);
        ApplicationResponse<UninstallOption> GetUninstallOptionById(Guid id);
        ApplicationResponse<IEnumerable<UninstallOption>> GetAllUninstallOptions();
        ApplicationResponse<bool> UpdateUninstallOption(UpdateUninstallOptionRequest request);
        ApplicationResponse<bool> DeleteUninstallOption(Guid id);
        ApplicationResponse<IEnumerable<UninstallOption>> FindUninstallOptionsByFields(Dictionary<string, object> filters);
    }
}
