using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.FitUser.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IFitUserService
    {
        ApplicationResponse<FitUser> CreateFitUser(AddFitUserRequest request);
        ApplicationResponse<FitUser> GetFitUserById(Guid id);
        ApplicationResponse<IEnumerable<FitUser>> GetAllFitUsers();
        ApplicationResponse<bool> UpdateFitUser(UpdateFitUserRequest request);
        ApplicationResponse<bool> DeleteFitUser(Guid id);
        ApplicationResponse<IEnumerable<FitUser>> FindFitUsersByFields(Dictionary<string, object> filters);
    }
}
