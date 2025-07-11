using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IBrandRepository
    {
        Brand CreateBrand(Brand brand);
        Brand GetBrandById(Guid id);
        IEnumerable<Brand> GetAllBrands();
        bool UpdateBrand(Brand brand);
        bool DeleteBrand(Guid id);
        IEnumerable<Brand> FindBrandsByFields(Dictionary<string, object> filters);
    }
}
