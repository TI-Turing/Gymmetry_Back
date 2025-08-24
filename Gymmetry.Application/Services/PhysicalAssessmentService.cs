using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.PhysicalAssessment.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class PhysicalAssessmentService : IPhysicalAssessmentService
    {
        private readonly IPhysicalAssessmentRepository _physicalAssessmentRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public PhysicalAssessmentService(IPhysicalAssessmentRepository physicalAssessmentRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _physicalAssessmentRepository = physicalAssessmentRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<PhysicalAssessment>> CreatePhysicalAssessmentAsync(AddPhysicalAssessmentRequest request)
        {
            try
            {
                // Obtener último registro activo para el usuario
                PhysicalAssessment? last = null;
                try
                {
                    var all = await _physicalAssessmentRepository.GetAllPhysicalAssessmentsAsync();
                    last = all
                        .Where(p => p.UserId == request.UserId && p.IsActive == true)
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefault();
                }
                catch { /* si falla, continuamos sin heredar */ }

                string? Inherit(string? incoming, string? previous)
                {
                    if (IsZeroString(incoming)) return previous;
                    return incoming;
                }

                var entity = new PhysicalAssessment
                {
                    Height = Inherit(request.Height, last?.Height),
                    Weight = Inherit(request.Weight, last?.Weight),
                    LeftArm = Inherit(request.LeftArm, last?.LeftArm),
                    RighArm = Inherit(request.RighArm, last?.RighArm),
                    LeftForearm = Inherit(request.LeftForearm, last?.LeftForearm),
                    RightForearm = Inherit(request.RightForearm, last?.RightForearm),
                    LeftThigh = Inherit(request.LeftThigh, last?.LeftThigh),
                    RightThigh = Inherit(request.RightThigh, last?.RightThigh),
                    LeftCalf = Inherit(request.LeftCalf, last?.LeftCalf),
                    RightCalf = Inherit(request.RightCalf, last?.RightCalf),
                    Abdomen = Inherit(request.Abdomen, last?.Abdomen),
                    Chest = Inherit(request.Chest, last?.Chest),
                    UpperBack = Inherit(request.UpperBack, last?.UpperBack),
                    LowerBack = Inherit(request.LowerBack, last?.LowerBack),
                    Neck = Inherit(request.Neck, last?.Neck),
                    Waist = Inherit(request.Waist, last?.Waist),
                    Hips = Inherit(request.Hips, last?.Hips),
                    Shoulders = Inherit(request.Shoulders, last?.Shoulders),
                    Wrist = Inherit(request.Wrist, last?.Wrist),
                    BodyFatPercentage = Inherit(request.BodyFatPercentage, last?.BodyFatPercentage),
                    MuscleMass = Inherit(request.MuscleMass, last?.MuscleMass),
                    Bmi = Inherit(request.Bmi, last?.Bmi),
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId
                };
                var created = await _physicalAssessmentRepository.CreatePhysicalAssessmentAsync(entity);
                return new ApplicationResponse<PhysicalAssessment>
                {
                    Success = true,
                    Message = "Evaluación física creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<PhysicalAssessment>
                {
                    Success = false,
                    Message = "Error técnico al crear la evaluación física.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        private static bool IsZeroString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false; // sólo heredar cuando viene explícitamente cero
            var v = value.Trim();
            if (v == "0" || v == "0.0" || v == "0,0" || v == "0.00" || v == "0,00") return true;
            if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d == 0m;
            if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d == 0m;
            return false;
        }

        public async Task<ApplicationResponse<PhysicalAssessment>> GetPhysicalAssessmentByIdAsync(Guid id)
        {
            var entity = await _physicalAssessmentRepository.GetPhysicalAssessmentByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<PhysicalAssessment>
                {
                    Success = false,
                    Message = "Evaluación física no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<PhysicalAssessment>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<PhysicalAssessment>>> GetAllPhysicalAssessmentsAsync()
        {
            var entities = await _physicalAssessmentRepository.GetAllPhysicalAssessmentsAsync();
            return new ApplicationResponse<IEnumerable<PhysicalAssessment>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdatePhysicalAssessmentAsync(UpdatePhysicalAssessmentRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _physicalAssessmentRepository.GetPhysicalAssessmentByIdAsync(request.Id);
                var entity = new PhysicalAssessment
                {
                    Id = request.Id,
                    Height = request.Height,
                    Weight = request.Weight,
                    LeftArm = request.LeftArm,
                    RighArm = request.RighArm,
                    LeftForearm = request.LeftForearm,
                    RightForearm = request.RightForearm,
                    LeftThigh = request.LeftThigh,
                    RightThigh = request.RightThigh,
                    LeftCalf = request.LeftCalf,
                    RightCalf = request.RightCalf,
                    Abdomen = request.Abdomen,
                    Chest = request.Chest,
                    UpperBack = request.UpperBack,
                    LowerBack = request.LowerBack,
                    Neck = request.Neck,
                    Waist = request.Waist,
                    Hips = request.Hips,
                    Shoulders = request.Shoulders,
                    Wrist = request.Wrist,
                    BodyFatPercentage = request.BodyFatPercentage,
                    MuscleMass = request.MuscleMass,
                    Bmi = request.Bmi,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId
                };
                var updated = await _physicalAssessmentRepository.UpdatePhysicalAssessmentAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("PhysicalAssessment", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Evaluación física actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la evaluación física (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la evaluación física.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeletePhysicalAssessmentAsync(Guid id)
        {
            try
            {
                var deleted = await _physicalAssessmentRepository.DeletePhysicalAssessmentAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Evaluación física eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Evaluación física no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la evaluación física.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<PhysicalAssessment>>> FindPhysicalAssessmentsByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _physicalAssessmentRepository.FindPhysicalAssessmentsByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<PhysicalAssessment>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
