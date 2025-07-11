using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Brand.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IBrandService
    {
        ApplicationResponse<Brand> CreateBrand(AddBrandRequest request);
        ApplicationResponse<Brand> GetBrandById(Guid id);
        ApplicationResponse<IEnumerable<Brand>> GetAllBrands();
        ApplicationResponse<bool> UpdateBrand(UpdateBrandRequest request);
        ApplicationResponse<bool> DeleteBrand(Guid id);
        ApplicationResponse<IEnumerable<Brand>> FindBrandsByFields(Dictionary<string, object> filters);
    }
}
