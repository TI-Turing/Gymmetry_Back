using System;

namespace FitGymApp.Domain.DTO.GymPlanSelectedType.Request
{
    public class AddGymPlanSelectedTypeRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
