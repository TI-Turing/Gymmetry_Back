using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Brand.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public BrandService(IBrandRepository brandRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _brandRepository = brandRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Brand> CreateBrand(AddBrandRequest request)
        {
            try
            {
                var brand = new Brand
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _brandRepository.CreateBrand(brand);
                return new ApplicationResponse<Brand>
                {
                    Success = true,
                    Message = "Marca creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Brand>
                {
                    Success = false,
                    Message = "Error técnico al crear la marca.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Brand> GetBrandById(Guid id)
        {
            var brand = _brandRepository.GetBrandById(id);
            if (brand == null)
            {
                return new ApplicationResponse<Brand>
                {
                    Success = false,
                    Message = "Marca no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Brand>
            {
                Success = true,
                Data = brand
            };
        }

        public ApplicationResponse<IEnumerable<Brand>> GetAllBrands()
        {
            var brands = _brandRepository.GetAllBrands();
            return new ApplicationResponse<IEnumerable<Brand>>
            {
                Success = true,
                Data = brands
            };
        }

        public ApplicationResponse<bool> UpdateBrand(UpdateBrandRequest request)
        {
            try
            {
                var brandBefore = _brandRepository.GetBrandById(request.Id);
                var brand = new Brand
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _brandRepository.UpdateBrand(brand);
                if (updated)
                {
                    _logChangeService.LogChange("Brand", brandBefore, brand.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Marca actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la marca (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la marca.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteBrand(Guid id)
        {
            try
            {
                var deleted = _brandRepository.DeleteBrand(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Marca eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Marca no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la marca.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Brand>> FindBrandsByFields(Dictionary<string, object> filters)
        {
            var brands = _brandRepository.FindBrandsByFields(filters);
            return new ApplicationResponse<IEnumerable<Brand>>
            {
                Success = true,
                Data = brands
            };
        }
    }
}
