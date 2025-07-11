using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelectedType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymPlanSelectedTypeService
    {
        ApplicationResponse<GymPlanSelectedType> CreateGymPlanSelectedType(AddGymPlanSelectedTypeRequest request);
        ApplicationResponse<GymPlanSelectedType> GetGymPlanSelectedTypeById(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelectedType>> GetAllGymPlanSelectedTypes();
        ApplicationResponse<bool> UpdateGymPlanSelectedType(UpdateGymPlanSelectedTypeRequest request);
        ApplicationResponse<bool> DeleteGymPlanSelectedType(Guid id);
        ApplicationResponse<IEnumerable<GymPlanSelectedType>> FindGymPlanSelectedTypesByFields(Dictionary<string, object> filters);
    }
}
