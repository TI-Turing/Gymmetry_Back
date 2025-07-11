using System;

namespace FitGymApp.Domain.DTO.AccessMethodType.Request
{
    public class AddAccessMethodTypeRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
