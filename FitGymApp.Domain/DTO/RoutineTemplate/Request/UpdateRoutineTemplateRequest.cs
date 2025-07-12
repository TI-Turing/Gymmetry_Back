using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.RoutineTemplate.Request
{
    public class UpdateRoutineTemplateRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Comments { get; set; } = null!;
        public Guid GymId { get; set; }
        public Guid RoutineUserRoutineId { get; set; }
        public Guid RoutineAssignedId { get; set; }
    }
}
