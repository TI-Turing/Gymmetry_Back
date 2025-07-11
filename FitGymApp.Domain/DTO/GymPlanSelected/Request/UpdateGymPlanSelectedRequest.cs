using System;

namespace FitGymApp.Domain.DTO.GymPlanSelected.Request
{
    public class UpdateGymPlanSelectedRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid GymPlanSelectedTypeId { get; set; }
    }
}
