using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.Models;

public partial class PhysicalAssessment
{
    public Guid Id { get; set; }

    public string? Height { get; set; }

    public string? Weight { get; set; }

    public string? LeftArm { get; set; }

    public string? RighArm { get; set; }

    public string? LeftForearm { get; set; }

    public string? RightForearm { get; set; }

    public string? LeftThigh { get; set; }

    public string? RightThigh { get; set; }

    public string? LeftCalf { get; set; }

    public string? RightCalf { get; set; }

    public string? Abdomen { get; set; }

    public string? Chest { get; set; }

    public string? UpperBack { get; set; }

    public string? LowerBack { get; set; }

    public string? Neck { get; set; }

    public string? Waist { get; set; }

    public string? Hips { get; set; }

    public string? Shoulders { get; set; }

    public string? Wrist { get; set; }

    public string? BodyFatPercentage { get; set; }

    public string? MuscleMass { get; set; }

    public string? Bmi { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Ip { get; set; }

    public bool? IsActive { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
