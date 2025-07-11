using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Diet.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class DietService : IDietService
    {
        private readonly IDietRepository _dietRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public DietService(IDietRepository dietRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _dietRepository = dietRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Diet> CreateDiet(AddDietRequest request)
        {
            try
            {
                var diet = new Diet
                {
                    BreakFast = request.BreakFast,
                    MidMorning = request.MidMorning,
                    Lunch = request.Lunch,
                    MidAfternoon = request.MidAfternoon,
                    Night = request.Night,
                    MidNight = request.MidNight,
                    Observations = request.Observations,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    UserId = request.UserId,
                    Ip = request.Ip
                };
                var created = _dietRepository.CreateDiet(diet);
                return new ApplicationResponse<Diet>
                {
                    Success = true,
                    Message = "Dieta creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Diet>
                {
                    Success = false,
                    Message = "Error técnico al crear la dieta.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Diet> GetDietById(Guid id)
        {
            var diet = _dietRepository.GetDietById(id);
            if (diet == null)
            {
                return new ApplicationResponse<Diet>
                {
                    Success = false,
                    Message = "Dieta no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Diet>
            {
                Success = true,
                Data = diet
            };
        }

        public ApplicationResponse<IEnumerable<Diet>> GetAllDiets()
        {
            var diets = _dietRepository.GetAllDiets();
            return new ApplicationResponse<IEnumerable<Diet>>
            {
                Success = true,
                Data = diets
            };
        }

        public ApplicationResponse<bool> UpdateDiet(UpdateDietRequest request)
        {
            try
            {
                var dietBefore = _dietRepository.GetDietById(request.Id);
                var diet = new Diet
                {
                    Id = request.Id,
                    BreakFast = request.BreakFast,
                    MidMorning = request.MidMorning,
                    Lunch = request.Lunch,
                    MidAfternoon = request.MidAfternoon,
                    Night = request.Night,
                    MidNight = request.MidNight,
                    Observations = request.Observations,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    UserId = request.UserId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _dietRepository.UpdateDiet(diet);
                if (updated)
                {
                    _logChangeService.LogChange("Diet", dietBefore, diet.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Dieta actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la dieta (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la dieta.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteDiet(Guid id)
        {
            try
            {
                var deleted = _dietRepository.DeleteDiet(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Dieta eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Dieta no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la dieta.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Diet>> FindDietsByFields(Dictionary<string, object> filters)
        {
            var diets = _dietRepository.FindDietsByFields(filters);
            return new ApplicationResponse<IEnumerable<Diet>>
            {
                Success = true,
                Data = diets
            };
        }
    }
}
