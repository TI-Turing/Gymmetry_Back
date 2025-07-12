using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymType.Request
{
    public class UpdateGymTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
