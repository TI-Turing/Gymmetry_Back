using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymPlanSelectedType.Request
{
    public class AddGymPlanSelectedTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}
