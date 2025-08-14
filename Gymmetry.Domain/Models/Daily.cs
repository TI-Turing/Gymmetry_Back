using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.Models;

public partial class Daily
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Ip { get; set; }

    public bool IsActive { get; set; }

    public Guid UserId { get; set; }

    public Guid RoutineDayId { get; set; } // Cambia la relación a RoutineDay

    public Guid BranchId { get; set; } // Nueva FK a Branch

    public Guid DailyExerciseId { get; set; } // FK a DailyExercise

    public virtual RoutineDay RoutineDay { get; set; } = null!; // Relación con RoutineDay

    public virtual Branch Branch { get; set; } = null!; // Relación con Branch

    public virtual User User { get; set; } = null!;

    public virtual DailyExercise DailyExercise { get; set; } = null!; // Relación 1 a 1
}
