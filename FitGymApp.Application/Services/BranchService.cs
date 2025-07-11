using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Branch.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public BranchService(IBranchRepository branchRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _branchRepository = branchRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Branch> CreateBranch(AddBranchRequest request)
        {
            try
            {
                var branch = new Branch
                {
                    Address = request.Address,
                    CityId = request.CityId,
                    RegionId = request.RegionId,
                    GymId = request.GymId,
                    BranchDailyBranchId = request.BranchDailyBranchId,
                    AccessMethodId = request.AccessMethodId,
                    Ip = request.Ip
                };
                var created = _branchRepository.CreateBranch(branch);
                return new ApplicationResponse<Branch>
                {
                    Success = true,
                    Message = "Sucursal creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Branch>
                {
                    Success = false,
                    Message = "Error técnico al crear la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Branch> GetBranchById(Guid id)
        {
            var branch = _branchRepository.GetBranchById(id);
            if (branch == null)
            {
                return new ApplicationResponse<Branch>
                {
                    Success = false,
                    Message = "Sucursal no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Branch>
            {
                Success = true,
                Data = branch
            };
        }

        public ApplicationResponse<IEnumerable<Branch>> GetAllBranches()
        {
            var branches = _branchRepository.GetAllBranches();
            return new ApplicationResponse<IEnumerable<Branch>>
            {
                Success = true,
                Data = branches
            };
        }

        public ApplicationResponse<bool> UpdateBranch(UpdateBranchRequest request)
        {
            try
            {
                var branchBefore = _branchRepository.GetBranchById(request.Id);
                var branch = new Branch
                {
                    Id = request.Id,
                    Address = request.Address,
                    CityId = request.CityId,
                    RegionId = request.RegionId,
                    GymId = request.GymId,
                    BranchDailyBranchId = request.BranchDailyBranchId,
                    AccessMethodId = request.AccessMethodId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _branchRepository.UpdateBranch(branch);
                if (updated)
                {
                    _logChangeService.LogChange("Branch", branchBefore, branch.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Sucursal actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la sucursal (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteBranch(Guid id)
        {
            try
            {
                var deleted = _branchRepository.DeleteBranch(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Sucursal eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Sucursal no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Branch>> FindBranchesByFields(Dictionary<string, object> filters)
        {
            var branches = _branchRepository.FindBranchesByFields(filters);
            return new ApplicationResponse<IEnumerable<Branch>>
            {
                Success = true,
                Data = branches
            };
        }
    }
}
