using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.SubModule.Request
{
    public class AddSubModuleRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
        public Guid ModuleId { get; set; }
        public Guid BranchId { get; set; }
    }
}
