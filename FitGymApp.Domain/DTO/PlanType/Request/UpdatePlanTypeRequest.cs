using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.PlanType.Request
{
    public class UpdatePlanTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
