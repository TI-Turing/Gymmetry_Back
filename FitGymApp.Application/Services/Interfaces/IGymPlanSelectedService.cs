using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelected.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymPlanSelectedService
    {
        ApplicationResponse<GymPlanSelected> CreateGymPlanSelected(AddGymPlanSelectedRequest request);
        ApplicationResponse<GymPlanSelected> GetGymPlanSelectedById(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelected>> GetAllGymPlanSelecteds();
        ApplicationResponse<bool> UpdateGymPlanSelected(UpdateGymPlanSelectedRequest request);
        ApplicationResponse<bool> DeleteGymPlanSelected(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelected>> FindGymPlanSelectedsByFields(Dictionary<string, object> filters);
    }
}
