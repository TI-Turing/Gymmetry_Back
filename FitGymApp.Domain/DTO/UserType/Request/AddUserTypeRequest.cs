using System;

namespace FitGymApp.Domain.DTO.UserType.Request
{
    public class AddUserTypeRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
