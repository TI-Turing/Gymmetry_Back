using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.Models;

public partial class Feed
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? Ip { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsAnonymous { get; set; } = false;

    // Métricas agregadas
    public int LikesCount { get; set; } = 0;
    public int CommentsCount { get; set; } = 0;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<FeedLike> FeedLikes { get; set; } = new List<FeedLike>();
    public virtual ICollection<FeedComment> FeedComments { get; set; } = new List<FeedComment>();
}
