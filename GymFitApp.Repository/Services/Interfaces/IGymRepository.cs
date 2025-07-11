using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IGymRepository
    {
        Gym CreateGym(Gym entity);
        Gym GetGymById(Guid id);
        IEnumerable<Gym> GetAllGyms();
        bool UpdateGym(Gym entity);
        bool DeleteGym(Guid id);
        IEnumerable<Gym> FindGymsByFields(Dictionary<string, object> filters);
    }
}
