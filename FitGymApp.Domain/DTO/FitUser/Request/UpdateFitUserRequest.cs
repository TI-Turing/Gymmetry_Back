using System;

namespace FitGymApp.Domain.DTO.FitUser.Request
{
    public class UpdateFitUserRequest
    {
        public Guid Id { get; set; }
        public string Goal { get; set; } = null!;
        public string ExperienceLevel { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
    }
}
