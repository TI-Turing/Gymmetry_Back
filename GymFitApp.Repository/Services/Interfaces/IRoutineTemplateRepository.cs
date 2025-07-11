using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IRoutineTemplateRepository
    {
        RoutineTemplate CreateRoutineTemplate(RoutineTemplate entity);
        RoutineTemplate GetRoutineTemplateById(Guid id);
        IEnumerable<RoutineTemplate> GetAllRoutineTemplates();
        bool UpdateRoutineTemplate(RoutineTemplate entity);
        bool DeleteRoutineTemplate(Guid id);
        IEnumerable<RoutineTemplate> FindRoutineTemplatesByFields(Dictionary<string, object> filters);
    }
}
