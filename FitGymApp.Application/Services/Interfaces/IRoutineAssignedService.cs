using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineAssigned.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IRoutineAssignedService
    {
        ApplicationResponse<RoutineAssigned> CreateRoutineAssigned(AddRoutineAssignedRequest request);
        ApplicationResponse<RoutineAssigned> GetRoutineAssignedById(Guid id);
        ApplicationResponse<IEnumerable<RoutineAssigned>> GetAllRoutineAssigneds();
        ApplicationResponse<bool> UpdateRoutineAssigned(UpdateRoutineAssignedRequest request);
        ApplicationResponse<bool> DeleteRoutineAssigned(Guid id);
        ApplicationResponse<IEnumerable<RoutineAssigned>> FindRoutineAssignedsByFields(Dictionary<string, object> filters);
    }
}
