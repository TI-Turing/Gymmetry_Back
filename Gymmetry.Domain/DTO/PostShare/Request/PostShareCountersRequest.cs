using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.PostShare.Request
{
    public class PostShareCountersRequest
    {
        [Required]
        public Guid PostId { get; set; }
    }
}