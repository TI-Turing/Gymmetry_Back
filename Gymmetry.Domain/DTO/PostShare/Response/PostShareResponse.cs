using System;

namespace Gymmetry.Domain.DTO.PostShare.Response
{
    public class PostShareResponse
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid SharedBy { get; set; }
        public Guid? SharedWith { get; set; }
        public string ShareType { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}