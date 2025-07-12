using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymPlanSelectedType.Request
{
    public class UpdateGymPlanSelectedTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
