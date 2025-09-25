using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.User.Request
{
    public class UploadUserProfileImageRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public byte[] Image { get; set; } = null!;
    }
}