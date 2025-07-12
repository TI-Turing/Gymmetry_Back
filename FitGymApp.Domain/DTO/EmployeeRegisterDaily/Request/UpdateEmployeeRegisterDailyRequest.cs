using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.EmployeeRegisterDaily.Request
{
    public class UpdateEmployeeRegisterDailyRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
