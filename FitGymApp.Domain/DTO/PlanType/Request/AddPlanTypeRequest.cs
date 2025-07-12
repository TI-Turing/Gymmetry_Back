using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.PlanType.Request
{
    public class AddPlanTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}
