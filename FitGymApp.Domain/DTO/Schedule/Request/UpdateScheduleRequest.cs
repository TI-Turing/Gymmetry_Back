using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.Schedule.Request
{
    public class UpdateScheduleRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHoliday { get; set; }
        public Guid BranchId { get; set; }
    }
}
