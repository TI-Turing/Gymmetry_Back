using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.EmployeeRegisterDaily.Request
{
    public class AddEmployeeRegisterDailyRequest : ApiRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
