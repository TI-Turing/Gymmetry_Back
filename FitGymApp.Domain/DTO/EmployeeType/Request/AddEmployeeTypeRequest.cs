using System;

namespace FitGymApp.Domain.DTO.EmployeeType.Request
{
    public class AddEmployeeTypeRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
