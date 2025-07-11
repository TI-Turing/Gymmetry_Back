using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.PhysicalAssessment.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IPhysicalAssessmentService
    {
        ApplicationResponse<PhysicalAssessment> CreatePhysicalAssessment(AddPhysicalAssessmentRequest request);
        ApplicationResponse<PhysicalAssessment> GetPhysicalAssessmentById(Guid id);
        ApplicationResponse<IEnumerable<PhysicalAssessment>> GetAllPhysicalAssessments();
        ApplicationResponse<bool> UpdatePhysicalAssessment(UpdatePhysicalAssessmentRequest request);
        ApplicationResponse<bool> DeletePhysicalAssessment(Guid id);
        ApplicationResponse<IEnumerable<PhysicalAssessment>> FindPhysicalAssessmentsByFields(Dictionary<string, object> filters);
    }
}
