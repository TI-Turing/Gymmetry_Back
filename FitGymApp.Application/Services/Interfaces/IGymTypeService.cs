using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IGymTypeService
    {
        ApplicationResponse<GymType> CreateGymType(AddGymTypeRequest request);
        ApplicationResponse<GymType> GetGymTypeById(Guid id);
        ApplicationResponse<IEnumerable<GymType>> GetAllGymTypes();
        ApplicationResponse<bool> UpdateGymType(UpdateGymTypeRequest request);
        ApplicationResponse<bool> DeleteGymType(Guid id);
        ApplicationResponse<IEnumerable<GymType>> FindGymTypesByFields(Dictionary<string, object> filters);
    }
}
