using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IPermissionRepository
    {
        Permission CreatePermission(Permission entity);
        Permission GetPermissionById(Guid id);
        IEnumerable<Permission> GetAllPermissions();
        bool UpdatePermission(Permission entity);
        bool DeletePermission(Guid id);
        IEnumerable<Permission> FindPermissionsByFields(Dictionary<string, object> filters);
    }
}
