using System;

namespace FitGymApp.Domain.DTO.GymType.Request
{
    public class UpdateGymTypeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
    }
}
