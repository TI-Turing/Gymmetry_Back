using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.Module.Request
{
    public class UpdateModuleRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Url { get; set; }
        public Guid UserTypeId { get; set; }
        public Guid GymPlanSelectedModuleModuleModuleId { get; set; }
    }
}
