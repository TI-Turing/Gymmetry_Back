using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.UserExerciseMax.Request
{
    public class AddUserExerciseMaxRequest : ApiRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid ExerciseId { get; set; }
        [Range(0, 1000)]
        public decimal WeightKg { get; set; }
        public DateTime? AchievedAt { get; set; }
        public string? Ip { get; set; }
    }
}
