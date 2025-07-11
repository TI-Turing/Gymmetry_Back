using System;

namespace FitGymApp.Domain.DTO.GymPlanSelected.Request
{
    public class AddGymPlanSelectedRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid GymPlanSelectedTypeId { get; set; }
    }
}
