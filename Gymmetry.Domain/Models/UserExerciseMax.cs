using System;

namespace Gymmetry.Domain.Models
{
    public class UserExerciseMax
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ExerciseId { get; set; }
        public decimal WeightKg { get; set; }
        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual User User { get; set; } = null!;
        public virtual Exercise Exercise { get; set; } = null!;
    }
}
