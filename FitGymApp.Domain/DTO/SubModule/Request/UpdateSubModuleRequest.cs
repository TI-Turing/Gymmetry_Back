using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.SubModule.Request
{
    public class UpdateSubModuleRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid ModuleId { get; set; }
        public Guid BranchId { get; set; }
    }
}
