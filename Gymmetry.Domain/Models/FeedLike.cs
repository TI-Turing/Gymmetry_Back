using System;

namespace Gymmetry.Domain.Models;

public class FeedLike
{
    public Guid Id { get; set; }
    public Guid FeedId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string? Ip { get; set; }

    public virtual Feed Feed { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
