using System;

namespace FitGymApp.Domain.DTO.GymPlanSelectedModule.Request
{
    public class AddGymPlanSelectedModuleRequest
    {
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid GymPlanSelectedId { get; set; }
    }
}
