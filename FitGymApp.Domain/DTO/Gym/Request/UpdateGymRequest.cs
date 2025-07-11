using System;

namespace FitGymApp.Domain.DTO.Gym.Request
{
    public class UpdateGymRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Nit { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid CountryId { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid GymPlanSelectedId { get; set; }
        public Guid GymTypeId { get; set; }
    }
}
