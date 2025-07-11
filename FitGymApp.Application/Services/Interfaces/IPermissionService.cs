using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Permission.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPermissionService
    {
        ApplicationResponse<Permission> CreatePermission(AddPermissionRequest request);
        ApplicationResponse<Permission> GetPermissionById(Guid id);
        ApplicationResponse<IEnumerable<Permission>> GetAllPermissions();
        ApplicationResponse<bool> UpdatePermission(UpdatePermissionRequest request);
        ApplicationResponse<bool> DeletePermission(Guid id);
        ApplicationResponse<IEnumerable<Permission>> FindPermissionsByFields(Dictionary<string, object> filters);
    }
}
