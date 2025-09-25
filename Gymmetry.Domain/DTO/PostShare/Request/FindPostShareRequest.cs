using System;

namespace Gymmetry.Domain.DTO.PostShare.Request
{
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
}