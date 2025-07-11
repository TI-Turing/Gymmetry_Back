using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymTypeRepository
    {
        GymType CreateGymType(GymType entity);
        GymType GetGymTypeById(Guid id);
        IEnumerable<GymType> GetAllGymTypes();
        bool UpdateGymType(GymType entity);
        bool DeleteGymType(Guid id);
        IEnumerable<GymType> FindGymTypesByFields(Dictionary<string, object> filters);
    }
}
