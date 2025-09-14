using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.Models
{
    public class UserBlock
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid BlockerId { get; set; }

        [Required]
        public Guid BlockedUserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(45)]
        public string? Ip { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        // Navigation properties
        public virtual User Blocker { get; set; } = null!;
        
        public virtual User BlockedUser { get; set; } = null!;
    }
}