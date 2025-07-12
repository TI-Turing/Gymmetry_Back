using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.MachineCategory.Request
{
    public class UpdateMachineCategoryRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
