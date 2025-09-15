using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.PostShare.Request
{
    public class AddPostShareRequest
    {
        [Required]
        public Guid PostId { get; set; }

        [Required]
        public Guid SharedBy { get; set; }

        public Guid? SharedWith { get; set; }

        [Required]
        [StringLength(16)]
        public string ShareType { get; set; } = string.Empty; // Internal, External

        [Required]
        [StringLength(16)]
        public string Platform { get; set; } = string.Empty; // App, WhatsApp, Instagram, Facebook, Twitter, SMS, Email, Other

        [StringLength(65536)] // Max 64KB
        public string? Metadata { get; set; } // JSON opcional
    }

    public class UpdatePostShareRequest
    {
        [Required]
        public Guid Id { get; set; }

        [StringLength(65536)] // Max 64KB
        public string? Metadata { get; set; }

        public bool? IsActive { get; set; }
    }

    public class FindPostShareRequest
    {
        public Guid? PostId { get; set; }
        public Guid? SharedBy { get; set; }
        public Guid? SharedWith { get; set; }
        public string? ShareType { get; set; }
        public string? Platform { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PostShareCountersRequest
    {
        [Required]
        public Guid PostId { get; set; }
    }
}