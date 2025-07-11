using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Plan.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPlanService
    {
        ApplicationResponse<Plan> CreatePlan(AddPlanRequest request);
        ApplicationResponse<Plan> GetPlanById(Guid id);
        ApplicationResponse<IEnumerable<Plan>> GetAllPlans();
        ApplicationResponse<bool> UpdatePlan(UpdatePlanRequest request);
        ApplicationResponse<bool> DeletePlan(Guid id);
        ApplicationResponse<IEnumerable<Plan>> FindPlansByFields(Dictionary<string, object> filters);
    }
}
