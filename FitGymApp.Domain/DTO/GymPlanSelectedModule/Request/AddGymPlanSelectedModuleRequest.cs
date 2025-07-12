using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymPlanSelectedModule.Request
{
    public class AddGymPlanSelectedModuleRequest : ApiRequest
    {
        public Guid GymPlanSelectedId { get; set; }
    }
}
