using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Feed.Request
{
    public class CreateFeedWithMediaRequest : ApiRequest
    {
        [Required(ErrorMessage = "La descripción es requerida para posts con multimedia")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string Description { get; set; } = null!;
        
        public bool IsAnonymous { get; set; } = false;
        
        [StringLength(500, ErrorMessage = "Los hashtags no pueden exceder 500 caracteres")]
        public string? Hashtags { get; set; }
        
        /// <summary>
        /// Archivos multimedia - máximo 5 archivos
        /// </summary>
        [Required(ErrorMessage = "Al menos un archivo multimedia es requerido")]
        public FileData[] Files { get; set; } = Array.Empty<FileData>();
    }

    public class FileData
    {
        public byte[] Content { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length => Content?.Length ?? 0;
    }
}