using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.PostShare.Request
{
    public class UpdatePostShareRequest
    {
        [Required]
        public Guid Id { get; set; }

        [StringLength(65536)] // Max 64KB
        public string? Metadata { get; set; }

        public bool? IsActive { get; set; }
    }
}