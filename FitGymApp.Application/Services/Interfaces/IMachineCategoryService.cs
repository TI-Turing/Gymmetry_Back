using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.MachineCategory.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IMachineCategoryService
    {
        ApplicationResponse<MachineCategory> CreateMachineCategory(AddMachineCategoryRequest request);
        ApplicationResponse<MachineCategory> GetMachineCategoryById(Guid id);
        ApplicationResponse<IEnumerable<MachineCategory>> GetAllMachineCategories();
        ApplicationResponse<bool> UpdateMachineCategory(UpdateMachineCategoryRequest request);
        ApplicationResponse<bool> DeleteMachineCategory(Guid id);
        ApplicationResponse<IEnumerable<MachineCategory>> FindMachineCategoriesByFields(Dictionary<string, object> filters);
    }
}
