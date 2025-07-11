using System;

namespace FitGymApp.Domain.DTO.EmployeeRegisterDaily.Request
{
    public class UpdateEmployeeRegisterDailyRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
    }
}
