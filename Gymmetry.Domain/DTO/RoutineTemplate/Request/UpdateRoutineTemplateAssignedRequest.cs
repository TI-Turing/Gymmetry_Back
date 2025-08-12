using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.RoutineTemplate.Request
{
    public class UpdateRoutineTemplateAssignedRequest
    {
        [Required]
        public Guid RoutineTemplateId { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}
