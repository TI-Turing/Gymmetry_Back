using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.CategoryExercise.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class CategoryExerciseService : ICategoryExerciseService
    {
        private readonly ICategoryExerciseRepository _categoryExerciseRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public CategoryExerciseService(ICategoryExerciseRepository categoryExerciseRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _categoryExerciseRepository = categoryExerciseRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<CategoryExercise> CreateCategoryExercise(AddCategoryExerciseRequest request)
        {
            try
            {
                var entity = new CategoryExercise
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _categoryExerciseRepository.CreateCategoryExercise(entity);
                return new ApplicationResponse<CategoryExercise>
                {
                    Success = true,
                    Message = "Categoría de ejercicio creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<CategoryExercise>
                {
                    Success = false,
                    Message = "Error técnico al crear la categoría de ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<CategoryExercise> GetCategoryExerciseById(Guid id)
        {
            var entity = _categoryExerciseRepository.GetCategoryExerciseById(id);
            if (entity == null)
            {
                return new ApplicationResponse<CategoryExercise>
                {
                    Success = false,
                    Message = "Categoría de ejercicio no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<CategoryExercise>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<CategoryExercise>> GetAllCategoryExercises()
        {
            var entities = _categoryExerciseRepository.GetAllCategoryExercises();
            return new ApplicationResponse<IEnumerable<CategoryExercise>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateCategoryExercise(UpdateCategoryExerciseRequest request)
        {
            try
            {
                var before = _categoryExerciseRepository.GetCategoryExerciseById(request.Id);
                var entity = new CategoryExercise
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _categoryExerciseRepository.UpdateCategoryExercise(entity);
                if (updated)
                {
                    _logChangeService.LogChange("CategoryExercise", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Categoría de ejercicio actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la categoría de ejercicio (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la categoría de ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteCategoryExercise(Guid id)
        {
            try
            {
                var deleted = _categoryExerciseRepository.DeleteCategoryExercise(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Categoría de ejercicio eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Categoría de ejercicio no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la categoría de ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<CategoryExercise>> FindCategoryExercisesByFields(Dictionary<string, object> filters)
        {
            var entities = _categoryExerciseRepository.FindCategoryExercisesByFields(filters);
            return new ApplicationResponse<IEnumerable<CategoryExercise>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
