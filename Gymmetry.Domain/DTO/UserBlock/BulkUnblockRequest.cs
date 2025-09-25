using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class BulkUnblockRequest : ApiRequest
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new();
    }
}