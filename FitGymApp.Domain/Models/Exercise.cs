using System;
using System.Collections.Generic;

namespace FitGymApp.Domain.Models;

public partial class Exercise
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Ip { get; set; }

    public bool IsActive { get; set; }

    public Guid CategoryExerciseId { get; set; }

    public Guid CategoryExerciseId1 { get; set; }

    public virtual DailyExerciseHistory CategoryExercise { get; set; } = null!;

    public virtual CategoryExercise CategoryExerciseId1Navigation { get; set; } = null!;

    public virtual ICollection<DailyExercise> DailyExercises { get; set; } = new List<DailyExercise>();

    public virtual ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();
}
