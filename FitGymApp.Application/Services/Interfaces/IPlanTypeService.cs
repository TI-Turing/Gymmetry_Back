using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.PlanType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPlanTypeService
    {
        ApplicationResponse<PlanType> CreatePlanType(AddPlanTypeRequest request);
        ApplicationResponse<PlanType> GetPlanTypeById(Guid id);
        ApplicationResponse<IEnumerable<PlanType>> GetAllPlanTypes();
        ApplicationResponse<bool> UpdatePlanType(UpdatePlanTypeRequest request);
        ApplicationResponse<bool> DeletePlanType(Guid id);
        ApplicationResponse<IEnumerable<PlanType>> FindPlanTypesByFields(Dictionary<string, object> filters);
    }
}
