using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Daily.Request
{
    public class AddDailyRequest : ApiRequest
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Range(0, 100)]
        public int Percentage { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid RoutineDayId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Ip { get; set; }
    }
}
