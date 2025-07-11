using System;

namespace FitGymApp.Domain.DTO.SubModule.Request
{
    public class UpdateSubModuleRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid ModuleId { get; set; }
        public Guid BranchId { get; set; }
    }
}
