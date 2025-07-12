using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.RoutineAssigned.Request
{
    public class UpdateRoutineAssignedRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string? Comments { get; set; }
        public Guid UserId { get; set; }
    }
}
