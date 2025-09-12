using System;

namespace Gymmetry.Domain.Models;

public class FeedComment
{
    public Guid Id { get; set; }
    public Guid FeedId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string? Ip { get; set; }
    public bool IsAnonymous { get; set; } = false;

    public virtual Feed Feed { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
