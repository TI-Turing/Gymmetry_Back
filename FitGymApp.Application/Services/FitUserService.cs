using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.FitUser.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class FitUserService : IFitUserService
    {
        private readonly IFitUserRepository _fitUserRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public FitUserService(IFitUserRepository fitUserRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _fitUserRepository = fitUserRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<FitUser> CreateFitUser(AddFitUserRequest request)
        {
            try
            {
                var entity = new FitUser
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _fitUserRepository.CreateFitUser(entity);
                return new ApplicationResponse<FitUser>
                {
                    Success = true,
                    Message = "FitUser creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<FitUser>
                {
                    Success = false,
                    Message = "Error técnico al crear el FitUser.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<FitUser> GetFitUserById(Guid id)
        {
            var entity = _fitUserRepository.GetFitUserById(id);
            if (entity == null)
            {
                return new ApplicationResponse<FitUser>
                {
                    Success = false,
                    Message = "FitUser no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<FitUser>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<FitUser>> GetAllFitUsers()
        {
            var entities = _fitUserRepository.GetAllFitUsers();
            return new ApplicationResponse<IEnumerable<FitUser>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateFitUser(UpdateFitUserRequest request)
        {
            try
            {
                var before = _fitUserRepository.GetFitUserById(request.Id);
                var entity = new FitUser
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _fitUserRepository.UpdateFitUser(entity);
                if (updated)
                {
                    _logChangeService.LogChange("FitUser", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "FitUser actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el FitUser (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el FitUser.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteFitUser(Guid id)
        {
            try
            {
                var deleted = _fitUserRepository.DeleteFitUser(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "FitUser eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "FitUser no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el FitUser.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<FitUser>> FindFitUsersByFields(Dictionary<string, object> filters)
        {
            var entities = _fitUserRepository.FindFitUsersByFields(filters);
            return new ApplicationResponse<IEnumerable<FitUser>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}
