using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class PhysicalAssessmentRepository : IPhysicalAssessmentRepository
    {
        private readonly FitGymAppContext _context;
        public PhysicalAssessmentRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public PhysicalAssessment CreatePhysicalAssessment(PhysicalAssessment assessment)
        {
            assessment.Id = Guid.NewGuid();
            assessment.CreatedAt = DateTime.UtcNow;
            assessment.IsActive = true;
            _context.PhysicalAssessments.Add(assessment);
            _context.SaveChanges();
            return assessment;
        }

        public PhysicalAssessment GetPhysicalAssessmentById(Guid id)
        {
            return _context.PhysicalAssessments.FirstOrDefault(a => a.Id == id && a.IsActive);
        }

        public IEnumerable<PhysicalAssessment> GetAllPhysicalAssessments()
        {
            return _context.PhysicalAssessments.Where(a => a.IsActive).ToList();
        }

        public bool UpdatePhysicalAssessment(PhysicalAssessment assessment)
        {
            var existing = _context.PhysicalAssessments.FirstOrDefault(a => a.Id == assessment.Id && a.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(assessment);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeletePhysicalAssessment(Guid id)
        {
            var entity = _context.PhysicalAssessments.FirstOrDefault(a => a.Id == id && a.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<PhysicalAssessment> FindPhysicalAssessmentsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(PhysicalAssessment), "a");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(PhysicalAssessment.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(PhysicalAssessment).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<PhysicalAssessment, bool>>(predicate, parameter);
            return _context.PhysicalAssessments.Where(lambda).ToList();
        }
    }
}
