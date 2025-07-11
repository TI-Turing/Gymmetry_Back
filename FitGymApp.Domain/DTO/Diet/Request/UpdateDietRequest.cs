using System;

namespace FitGymApp.Domain.DTO.Diet.Request
{
    public class UpdateDietRequest
    {
        public Guid Id { get; set; }
        public string BreakFast { get; set; } = null!;
        public string MidMorning { get; set; } = null!;
        public string Lunch { get; set; } = null!;
        public string MidAfternoon { get; set; } = null!;
        public string Night { get; set; } = null!;
        public string MidNight { get; set; } = null!;
        public string? Observations { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
    }
}
