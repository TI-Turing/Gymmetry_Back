using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Gym.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymService
    {
        ApplicationResponse<Gym> CreateGym(AddGymRequest request);
        ApplicationResponse<Gym> GetGymById(Guid id);
        ApplicationResponse<IEnumerable<Gym>> GetAllGyms();
        ApplicationResponse<bool> UpdateGym(UpdateGymRequest request);
        ApplicationResponse<bool> DeleteGym(Guid id);
        ApplicationResponse<IEnumerable<Gym>> FindGymsByFields(Dictionary<string, object> filters);
    }
}
