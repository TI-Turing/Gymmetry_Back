using System;

namespace FitGymApp.Domain.DTO.Plan.Request
{
    public class UpdatePlanRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid GymId { get; set; }
        public Guid PlanTypeId { get; set; }
    }
}
