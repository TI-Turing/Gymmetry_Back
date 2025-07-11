using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IPhysicalAssessmentRepository
    {
        PhysicalAssessment CreatePhysicalAssessment(PhysicalAssessment assessment);
        PhysicalAssessment GetPhysicalAssessmentById(Guid id);
        IEnumerable<PhysicalAssessment> GetAllPhysicalAssessments();
        bool UpdatePhysicalAssessment(PhysicalAssessment assessment);
        bool DeletePhysicalAssessment(Guid id);
        IEnumerable<PhysicalAssessment> FindPhysicalAssessmentsByFields(Dictionary<string, object> filters);
    }
}
