using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.Models;

public partial class RoutineAssigned
{
    public Guid Id { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Ip { get; set; }

    public bool IsActive { get; set; }

    public Guid UserId { get; set; }

    public Guid RoutineTemplateId { get; set; }
    public virtual RoutineTemplate RoutineTemplate { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
