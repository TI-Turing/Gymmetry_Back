using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Diet.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IDietService
    {
        ApplicationResponse<Diet> CreateDiet(AddDietRequest request);
        ApplicationResponse<Diet> GetDietById(Guid id);
        ApplicationResponse<IEnumerable<Diet>> GetAllDiets();
        ApplicationResponse<bool> UpdateDiet(UpdateDietRequest request);
        ApplicationResponse<bool> DeleteDiet(Guid id);
        ApplicationResponse<IEnumerable<Diet>> FindDietsByFields(Dictionary<string, object> filters);
    }
}
