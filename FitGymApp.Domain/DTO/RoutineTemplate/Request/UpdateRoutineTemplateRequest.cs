using System;

namespace FitGymApp.Domain.DTO.RoutineTemplate.Request
{
    public class UpdateRoutineTemplateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Comments { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid GymId { get; set; }
        public Guid RoutineUserRoutineId { get; set; }
        public Guid RoutineAssignedId { get; set; }
    }
}
