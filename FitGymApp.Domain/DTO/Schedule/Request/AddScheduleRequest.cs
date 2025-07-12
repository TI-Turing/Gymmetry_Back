using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.Schedule.Request
{
    public class AddScheduleRequest : ApiRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHoliday { get; set; }
        public Guid BranchId { get; set; }
    }
}
