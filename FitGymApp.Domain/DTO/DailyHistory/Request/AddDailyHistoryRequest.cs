using System;

namespace FitGymApp.Domain.DTO.DailyHistory.Request
{
    public class AddDailyHistoryRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
        public Guid RoutineExerciseId { get; set; }
    }
}
