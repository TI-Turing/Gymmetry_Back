using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserExerciseMax.Request;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class UserExerciseMaxService : IUserExerciseMaxService
    {
        private readonly IUserExerciseMaxRepository _repository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public UserExerciseMaxService(IUserExerciseMaxRepository repository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _repository = repository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<UserExerciseMax>> CreateAsync(AddUserExerciseMaxRequest request)
        {
            try
            {
                var entity = new UserExerciseMax
                {
                    UserId = request.UserId,
                    ExerciseId = request.ExerciseId,
                    WeightKg = request.WeightKg,
                    AchievedAt = request.AchievedAt ?? DateTime.UtcNow,
                    Ip = request.Ip,
                    IsActive = true
                };
                var created = await _repository.CreateAsync(entity);
                return new ApplicationResponse<UserExerciseMax>
                {
                    Success = true,
                    Message = "UserExerciseMax creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<UserExerciseMax>
                {
                    Success = false,
                    Message = "Error técnico al crear el UserExerciseMax.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<UserExerciseMax>> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<UserExerciseMax>
                {
                    Success = false,
                    Message = "UserExerciseMax no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<UserExerciseMax>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<UserExerciseMax>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return new ApplicationResponse<IEnumerable<UserExerciseMax>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateAsync(UpdateUserExerciseMaxRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _repository.GetByIdAsync(request.Id);
                var entity = new UserExerciseMax
                {
                    Id = request.Id,
                    UserId = request.UserId,
                    ExerciseId = request.ExerciseId,
                    WeightKg = request.WeightKg,
                    AchievedAt = request.AchievedAt ?? DateTime.UtcNow
                };
                var updated = await _repository.UpdateAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("UserExerciseMax", before, userId, ip, invocationId);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "UserExerciseMax actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el UserExerciseMax (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar UserExerciseMax.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "UserExerciseMax eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "UserExerciseMax no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar UserExerciseMax.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserExerciseMax>>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _repository.FindByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<UserExerciseMax>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
