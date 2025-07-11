using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IAccessMethodTypeRepository
    {
        AccessMethodType CreateAccessMethodType(AccessMethodType entity);
        AccessMethodType GetAccessMethodTypeById(Guid id);
        IEnumerable<AccessMethodType> GetAllAccessMethodTypes();
        bool UpdateAccessMethodType(AccessMethodType entity);
        bool DeleteAccessMethodType(Guid id);
        IEnumerable<AccessMethodType> FindAccessMethodTypesByFields(Dictionary<string, object> filters);
    }
}
