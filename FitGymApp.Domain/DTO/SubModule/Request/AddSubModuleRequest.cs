using System;

namespace FitGymApp.Domain.DTO.SubModule.Request
{
    public class AddSubModuleRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid ModuleId { get; set; }
        public Guid BranchId { get; set; }
    }
}
