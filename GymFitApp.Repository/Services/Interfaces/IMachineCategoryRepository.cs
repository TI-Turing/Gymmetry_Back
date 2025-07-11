using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IMachineCategoryRepository
    {
        MachineCategory CreateMachineCategory(MachineCategory entity);
        MachineCategory GetMachineCategoryById(Guid id);
        IEnumerable<MachineCategory> GetAllMachineCategories();
        bool UpdateMachineCategory(MachineCategory entity);
        bool DeleteMachineCategory(Guid id);
        IEnumerable<MachineCategory> FindMachineCategoriesByFields(Dictionary<string, object> filters);
    }
}
