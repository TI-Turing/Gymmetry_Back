using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Feed.Request
{
    public class FeedCreateRequestDto : ApiRequest
    {
        public Guid UserId { get; set; }
        
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = null!;
        
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Media como string base64. Use este campo para enviar la imagen/video en JSON.
        /// </summary>
        public string? MediaBase64 { get; set; }
        
        /// <summary>
        /// Tipo de media (image/jpeg, video/mp4, etc.)
        /// </summary>
        [StringLength(50)]
        public string? MediaType { get; set; }
        
        /// <summary>
        /// Nombre del archivo opcional
        /// </summary>
        [StringLength(255)]
        public string? FileName { get; set; }
        
        /// <summary>
        /// Convierte MediaBase64 a byte[] si está presente
        /// </summary>
        public byte[]? GetMediaBytes()
        {
            if (string.IsNullOrEmpty(MediaBase64))
                return null;
                
            try
            {
                return Convert.FromBase64String(MediaBase64);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Establece media desde byte array como base64
        /// </summary>
        public void SetMediaBytes(byte[]? mediaBytes)
        {
            MediaBase64 = mediaBytes != null ? Convert.ToBase64String(mediaBytes) : null;
        }
    }
}