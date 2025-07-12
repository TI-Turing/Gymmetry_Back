using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.MachineCategory.Request
{
    public class AddMachineCategoryRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}
