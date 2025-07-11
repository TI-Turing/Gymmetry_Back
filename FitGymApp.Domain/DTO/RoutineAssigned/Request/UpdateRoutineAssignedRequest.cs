using System;

namespace FitGymApp.Domain.DTO.RoutineAssigned.Request
{
    public class UpdateRoutineAssignedRequest
    {
        public Guid Id { get; set; }
        public string? Comments { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
    }
}
