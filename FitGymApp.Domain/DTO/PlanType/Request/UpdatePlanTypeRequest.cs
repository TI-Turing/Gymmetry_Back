using System;

namespace FitGymApp.Domain.DTO.PlanType.Request
{
    public class UpdatePlanTypeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
    }
}
