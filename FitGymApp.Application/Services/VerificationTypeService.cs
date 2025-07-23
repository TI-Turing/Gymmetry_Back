using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class VerificationTypeService : IVerificationTypeService
    {
        private readonly IVerificationTypeRepository _verificationTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public VerificationTypeService(IVerificationTypeRepository verificationTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _verificationTypeRepository = verificationTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<VerificationType>> CreateVerificationTypeAsync(VerificationType request)
        {
            try
            {
                var entity = new VerificationType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = await _verificationTypeRepository.CreateVerificationTypeAsync(entity);
                return new ApplicationResponse<VerificationType>
                {
                    Success = true,
                    Message = "Tipo de verificaci�n creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<VerificationType>
                {
                    Success = false,
                    Message = "Error t�cnico al crear el tipo de verificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<VerificationType>> GetVerificationTypeByIdAsync(Guid id)
        {
            var entity = await _verificationTypeRepository.GetVerificationTypeByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<VerificationType>
                {
                    Success = false,
                    Message = "Tipo de verificaci�n no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<VerificationType>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<VerificationType>>> GetAllVerificationTypesAsync()
        {
            var entities = await _verificationTypeRepository.GetAllVerificationTypesAsync();
            return new ApplicationResponse<IEnumerable<VerificationType>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateVerificationTypeAsync(VerificationType request)
        {
            try
            {
                var before = await _verificationTypeRepository.GetVerificationTypeByIdAsync(request.Id);
                var entity = new VerificationType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = await _verificationTypeRepository.UpdateVerificationTypeAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("VerificationType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de verificaci�n actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de verificaci�n (no encontrado o inactivo).",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error t�cnico al actualizar el tipo de verificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteVerificationTypeAsync(Guid id)
        {
            try
            {
                var deleted = await _verificationTypeRepository.DeleteVerificationTypeAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de verificaci�n eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de verificaci�n no encontrado o ya eliminado.",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error t�cnico al eliminar el tipo de verificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<VerificationType>>> FindVerificationTypesByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _verificationTypeRepository.FindVerificationTypesByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<VerificationType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
