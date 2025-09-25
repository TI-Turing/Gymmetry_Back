using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class BulkModerationRequest : ApiRequest
    {
        [Required]
        public List<Guid> ModerationIds { get; set; } = new();

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}