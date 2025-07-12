using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.JourneyEmployee.Request
{
    public class AddJourneyEmployeeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
        public string StartHour { get; set; } = null!;
        public string EndHour { get; set; } = null!;
        public Guid EmployeeUserId { get; set; }
    }
}
