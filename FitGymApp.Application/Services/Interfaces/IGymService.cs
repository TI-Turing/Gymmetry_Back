using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Gym.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymService
    {
        Task<ApplicationResponse<Gym>> CreateGymAsync(AddGymRequest request);
        Task<ApplicationResponse<Gym>> GetGymByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<Gym>>> GetAllGymsAsync();
        Task<ApplicationResponse<bool>> UpdateGymAsync(UpdateGymRequest request);
        Task<ApplicationResponse<bool>> DeleteGymAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<Gym>>> FindGymsByFieldsAsync(Dictionary<string, object> filters);
    }
}
