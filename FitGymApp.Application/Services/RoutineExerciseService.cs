using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineExercise.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class RoutineExerciseService : IRoutineExerciseService
    {
        private readonly IRoutineExerciseRepository _routineExerciseRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public RoutineExerciseService(IRoutineExerciseRepository routineExerciseRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _routineExerciseRepository = routineExerciseRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<RoutineExercise> CreateRoutineExercise(AddRoutineExerciseRequest request)
        {
            try
            {
                var entity = new RoutineExercise
                {
                    Name = request.Name,
                    Description = request.Description,
                    UserId = request.UserId,
                    Ip = request.Ip
                };
                var created = _routineExerciseRepository.CreateRoutineExercise(entity);
                return new ApplicationResponse<RoutineExercise>
                {
                    Success = true,
                    Message = "Ejercicio de rutina creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<RoutineExercise>
                {
                    Success = false,
                    Message = "Error técnico al crear el ejercicio de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<RoutineExercise> GetRoutineExerciseById(Guid id)
        {
            var entity = _routineExerciseRepository.GetRoutineExerciseById(id);
            if (entity == null)
            {
                return new ApplicationResponse<RoutineExercise>
                {
                    Success = false,
                    Message = "Ejercicio de rutina no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<RoutineExercise>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<RoutineExercise>> GetAllRoutineExercises()
        {
            var entities = _routineExerciseRepository.GetAllRoutineExercises();
            return new ApplicationResponse<IEnumerable<RoutineExercise>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateRoutineExercise(UpdateRoutineExerciseRequest request)
        {
            try
            {
                var before = _routineExerciseRepository.GetRoutineExerciseById(request.Id);
                var entity = new RoutineExercise
                {
                    Id = request.Id,
                    Name = request.Name,
                    Description = request.Description,
                    UserId = request.UserId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _routineExerciseRepository.UpdateRoutineExercise(entity);
                if (updated)
                {
                    _logChangeService.LogChange("RoutineExercise", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio de rutina actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el ejercicio de rutina (no encontrado o inactivo).",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar el ejercicio de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteRoutineExercise(Guid id)
        {
            try
            {
                var deleted = _routineExerciseRepository.DeleteRoutineExercise(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio de rutina eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Ejercicio de rutina no encontrado o ya eliminado.",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar el ejercicio de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<RoutineExercise>> FindRoutineExercisesByFields(Dictionary<string, object> filters)
        {
            var entities = _routineExerciseRepository.FindRoutineExercisesByFields(filters);
            return new ApplicationResponse<IEnumerable<RoutineExercise>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
