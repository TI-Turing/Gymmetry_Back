using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymType.Request
{
    public class AddGymTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
