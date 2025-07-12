using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.EmployeeType.Request
{
    public class UpdateEmployeeTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
