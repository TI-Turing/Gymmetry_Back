using System;

namespace FitGymApp.Domain.DTO.EmployeeType.Request
{
    public class UpdateEmployeeTypeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
    }
}
