using System;

namespace FitGymApp.Domain.DTO.JourneyEmployee.Request
{
    public class UpdateJourneyEmployeeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string StartHour { get; set; } = null!;
        public string EndHour { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid EmployeeUserId { get; set; }
    }
}
