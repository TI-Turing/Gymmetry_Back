using System;

namespace FitGymApp.Domain.DTO.GymPlanSelectedModule.Request
{
    public class UpdateGymPlanSelectedModuleRequest
    {
        public Guid Id { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid GymPlanSelectedId { get; set; }
    }
}
