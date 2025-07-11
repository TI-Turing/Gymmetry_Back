using System;

namespace FitGymApp.Domain.DTO.MachineCategory.Request
{
    public class AddMachineCategoryRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
