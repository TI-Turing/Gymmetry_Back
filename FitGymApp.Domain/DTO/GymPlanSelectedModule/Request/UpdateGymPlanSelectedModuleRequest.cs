using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.GymPlanSelectedModule.Request
{
    public class UpdateGymPlanSelectedModuleRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public Guid GymPlanSelectedId { get; set; }
    }
}
