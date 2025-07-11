using System;

namespace FitGymApp.Domain.DTO.RoutineAssigned.Request
{
    public class AddRoutineAssignedRequest
    {
        public string? Comments { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
    }
}
