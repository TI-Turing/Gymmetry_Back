using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Branch.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public BranchService(IBranchRepository branchRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _branchRepository = branchRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public async Task<ApplicationResponse<Branch>> CreateBranchAsync(AddBranchRequest request)
        {
            try
            {
                var branch = _mapper.Map<Branch>(request);
                var created = await _branchRepository.CreateBranchAsync(branch);
                return new ApplicationResponse<Branch>
                {
                    Success = true,
                    Message = "Sucursal creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<Branch>
                {
                    Success = false,
                    Message = "Error técnico al crear la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<Branch>> GetBranchByIdAsync(Guid id)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(id);
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

        public async Task<ApplicationResponse<IEnumerable<Branch>>> GetAllBranchesAsync()
        {
            var branches = await _branchRepository.GetAllBranchesAsync();
            return new ApplicationResponse<IEnumerable<Branch>>
            {
                Success = true,
                Data = branches
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateBranchAsync(UpdateBranchRequest request)
        {
            try
            {
                var branchBefore = await _branchRepository.GetBranchByIdAsync(request.Id);
                var branch = _mapper.Map<Branch>(request);
                var updated = await _branchRepository.UpdateBranchAsync(branch);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("Branch", branchBefore, branch.Id);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteBranchAsync(Guid id)
        {
            try
            {
                var deleted = await _branchRepository.DeleteBranchAsync(id);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar la sucursal.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Branch>>> FindBranchesByFieldsAsync(Dictionary<string, object> filters)
        {
            var branches = await _branchRepository.FindBranchesByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<Branch>>
            {
                Success = true,
                Data = branches
            };
        }
    }
}
