using System;

namespace FitGymApp.Domain.DTO.JourneyEmployee.Request
{
    public class AddJourneyEmployeeRequest
    {
        public string Name { get; set; } = null!;
        public string StartHour { get; set; } = null!;
        public string EndHour { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid EmployeeUserId { get; set; }
    }
}
