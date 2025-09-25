using System;

namespace Gymmetry.Domain.DTO.AppState
{
    public class PlanInfoDto
    {
        public Guid? PlanId { get; set; }
        public string? PlanTypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int DaysRemaining { get; set; }
    }
}