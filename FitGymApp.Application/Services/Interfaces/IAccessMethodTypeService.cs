using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.AccessMethodType.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IAccessMethodTypeService
    {
        ApplicationResponse<AccessMethodType> CreateAccessMethodType(AddAccessMethodTypeRequest request);
        ApplicationResponse<AccessMethodType> GetAccessMethodTypeById(Guid id);
        ApplicationResponse<IEnumerable<AccessMethodType>> GetAllAccessMethodTypes();
        ApplicationResponse<bool> UpdateAccessMethodType(UpdateAccessMethodTypeRequest request);
        ApplicationResponse<bool> DeleteAccessMethodType(Guid id);
        ApplicationResponse<IEnumerable<AccessMethodType>> FindAccessMethodTypesByFields(Dictionary<string, object> filters);
    }
}
