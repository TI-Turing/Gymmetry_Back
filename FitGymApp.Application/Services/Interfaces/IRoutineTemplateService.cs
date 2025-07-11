using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineTemplate.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IRoutineTemplateService
    {
        ApplicationResponse<RoutineTemplate> CreateRoutineTemplate(AddRoutineTemplateRequest request);
        ApplicationResponse<RoutineTemplate> GetRoutineTemplateById(Guid id);
        ApplicationResponse<IEnumerable<RoutineTemplate>> GetAllRoutineTemplates();
        ApplicationResponse<bool> UpdateRoutineTemplate(UpdateRoutineTemplateRequest request);
        ApplicationResponse<bool> DeleteRoutineTemplate(Guid id);
        ApplicationResponse<IEnumerable<RoutineTemplate>> FindRoutineTemplatesByFields(Dictionary<string, object> filters);
    }
}
