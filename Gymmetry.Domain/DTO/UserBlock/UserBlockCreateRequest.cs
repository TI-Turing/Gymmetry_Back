using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockCreateRequest : ApiRequest
    {
        [Required]
        public Guid BlockedUserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}