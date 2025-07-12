using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.EmployeeType.Request
{
    public class AddEmployeeTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}