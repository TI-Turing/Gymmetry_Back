using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IFitUserRepository
    {
        FitUser CreateFitUser(FitUser entity);
        FitUser GetFitUserById(Guid id);
        IEnumerable<FitUser> GetAllFitUsers();
        bool UpdateFitUser(FitUser entity);
        bool DeleteFitUser(Guid id);
        IEnumerable<FitUser> FindFitUsersByFields(Dictionary<string, object> filters);
    }
}
