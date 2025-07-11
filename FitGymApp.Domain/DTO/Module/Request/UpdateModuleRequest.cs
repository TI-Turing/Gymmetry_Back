using System;

namespace FitGymApp.Domain.DTO.Module.Request
{
    public class UpdateModuleRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Url { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserTypeId { get; set; }
        public Guid GymPlanSelectedModuleModuleModuleId { get; set; }
    }
}
