using System;

namespace FitGymApp.Domain.DTO.Module.Request
{
    public class AddModuleRequest
    {
        public string Name { get; set; } = null!;
        public string? Url { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserTypeId { get; set; }
        public Guid GymPlanSelectedModuleModuleModuleId { get; set; }
    }
}
