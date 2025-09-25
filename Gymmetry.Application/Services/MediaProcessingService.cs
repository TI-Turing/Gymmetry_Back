using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Feed.Request;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class MediaProcessingService : IMediaProcessingService
    {
        private readonly ILogger<MediaProcessingService> _logger;
        
        private static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedVideoTypes = { ".mp4", ".mov" };
        
        public MediaProcessingService(ILogger<MediaProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<MediaValidationResult> ValidateMediaFileAsync(FileData file)
        {
            try
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (AllowedImageTypes.Contains(fileExtension))
                {
                    return new MediaValidationResult
                    {
                        IsValid = true,
                        MediaType = "image",
                        ContentType = file.ContentType
                    };
                }
                
                if (AllowedVideoTypes.Contains(fileExtension))
                {
                    return new MediaValidationResult
                    {
                        IsValid = true,
                        MediaType = "video",
                        ContentType = file.ContentType
                    };
                }
                
                return new MediaValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Formato de archivo no permitido: {fileExtension}. Formatos permitidos: {string.Join(", ", AllowedImageTypes.Concat(AllowedVideoTypes))}",
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating media file {FileName}", file.FileName);
                return new MediaValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Error validando el archivo",
                    ContentType = file.ContentType
                };
            }
        }

        public async Task<ProcessedMediaFile> ProcessImageAsync(FileData file, MediaProcessingOptions options)
        {
            try
            {
                // Para esta implementación básica, validamos tamaño y pasamos el archivo sin procesar
                // TODO: Implementar compresión real con ImageSharp cuando se agregue el paquete
                
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                // Validar tamaño para imágenes (500KB máximo)
                const int maxImageSize = 500 * 1024; // 500KB
                if (file.Content.Length > maxImageSize)
                {
                    throw new InvalidOperationException($"La imagen {file.FileName} excede el tamaño máximo de {maxImageSize / 1024}KB");
                }

                // Por ahora, retornamos el archivo original
                // TODO: Implementar redimensionamiento y compresión real
                var processedData = file.Content;
                
                _logger.LogInformation("Image processed: {FileName}, size: {Size} bytes", 
                    file.FileName, processedData.Length);

                return new ProcessedMediaFile
                {
                    Data = processedData,
                    ContentType = file.ContentType,
                    OriginalFileName = file.FileName,
                    SizeBytes = processedData.Length,
                    Width = 1920, // TODO: Obtener dimensiones reales
                    Height = 1080, // TODO: Obtener dimensiones reales
                    FileExtension = fileExtension
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image {FileName}", file.FileName);
                throw new InvalidOperationException($"Error procesando imagen {file.FileName}: {ex.Message}", ex);
            }
        }

        public async Task<ProcessedMediaFile> ProcessVideoAsync(FileData file, MediaProcessingOptions options)
        {
            try
            {
                // Validar tamaño para videos (15MB máximo)
                const int maxVideoSize = 15 * 1024 * 1024; // 15MB
                if (file.Content.Length > maxVideoSize)
                {
                    throw new InvalidOperationException($"El video {file.FileName} excede el tamaño máximo de {maxVideoSize / (1024 * 1024)}MB");
                }

                // Por ahora, pasamos el video sin procesar
                // TODO: Implementar compresión de video con FFMpeg
                
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                _logger.LogInformation("Video processed: {FileName}, size: {Size} bytes", 
                    file.FileName, file.Content.Length);

                return new ProcessedMediaFile
                {
                    Data = file.Content,
                    ContentType = file.ContentType,
                    OriginalFileName = file.FileName,
                    SizeBytes = file.Content.Length,
                    DurationSeconds = 60, // TODO: Obtener duración real con FFMpeg
                    Width = 1280, // TODO: Obtener dimensiones reales
                    Height = 720, // TODO: Obtener dimensiones reales
                    FileExtension = fileExtension
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing video {FileName}", file.FileName);
                throw new InvalidOperationException($"Error procesando video {file.FileName}: {ex.Message}", ex);
            }
        }

        public async Task<(int width, int height)> GetImageDimensionsAsync(FileData file)
        {
            // TODO: Implementar con ImageSharp para obtener dimensiones reales
            await Task.CompletedTask;
            return (1920, 1080); // Valores por defecto
        }

        public async Task<(int width, int height, int durationSeconds)> GetVideoInfoAsync(FileData file)
        {
            // TODO: Implementar con FFMpeg para obtener información real del video
            await Task.CompletedTask;
            return (1280, 720, 60); // Valores por defecto
        }
    }
}