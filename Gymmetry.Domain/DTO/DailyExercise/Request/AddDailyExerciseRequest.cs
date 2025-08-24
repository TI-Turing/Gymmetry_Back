using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.DailyExercise.Request
{
    public class AddDailyExerciseRequest : ApiRequest
    {
        [Required]
        public string Set { get; set; } = string.Empty;
        [Required]
        public string Repetitions { get; set; } = string.Empty;
        [Required]
        public Guid ExerciseId { get; set; }
        [Required]
        public Guid DailyId { get; set; }
    }
}
