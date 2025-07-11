using System;

namespace FitGymApp.Domain.DTO.Daily.Request
{
    public class UpdateDailyRequest
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public Guid RoutineExerciseId { get; set; }
    }
}
