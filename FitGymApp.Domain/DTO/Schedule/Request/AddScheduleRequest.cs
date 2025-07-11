using System;

namespace FitGymApp.Domain.DTO.Schedule.Request
{
    public class AddScheduleRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHoliday { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid BranchId { get; set; }
    }
}
