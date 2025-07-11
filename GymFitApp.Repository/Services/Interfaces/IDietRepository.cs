using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDietRepository
    {
        Diet CreateDiet(Diet diet);
        Diet GetDietById(Guid id);
        IEnumerable<Diet> GetAllDiets();
        bool UpdateDiet(Diet diet);
        bool DeleteDiet(Guid id);
        IEnumerable<Diet> FindDietsByFields(Dictionary<string, object> filters);
    }
}
