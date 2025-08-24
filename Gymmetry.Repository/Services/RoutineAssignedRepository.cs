using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Repository.Services
{
    public class RoutineAssignedRepository : IRoutineAssignedRepository
    {
        private readonly GymmetryContext _context;
        public RoutineAssignedRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<RoutineAssigned> CreateRoutineAssignedAsync(RoutineAssigned entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.RoutineAssigneds.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<RoutineAssigned?> GetRoutineAssignedByIdAsync(Guid id)
        {
            return await _context.RoutineAssigneds
                .AsNoTracking()
                .Include(e => e.RoutineTemplate)
                    .ThenInclude(rt => rt.RoutineDays)
                .Include(e => e.RoutineTemplate)
                    .ThenInclude(rt => rt.RoutineExercises)
                .Where(e => e.Id == id && e.IsActive)
                .Select(e => new RoutineAssigned
                {
                    Id = e.Id,
                    Comments = e.Comments,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Ip = e.Ip,
                    IsActive = e.IsActive,
                    UserId = e.UserId,
                    RoutineTemplateId = e.RoutineTemplateId,
                    RoutineTemplate = new RoutineTemplate
                    {
                        Id = e.RoutineTemplate.Id,
                        Name = e.RoutineTemplate.Name,
                        Comments = e.RoutineTemplate.Comments,
                        CreatedAt = e.RoutineTemplate.CreatedAt,
                        UpdatedAt = e.RoutineTemplate.UpdatedAt,
                        IsActive = e.RoutineTemplate.IsActive,
                        Premium = e.RoutineTemplate.Premium,
                        IsDefault = e.RoutineTemplate.IsDefault,
                        TagsObjectives = e.RoutineTemplate.TagsObjectives,
                        TagsMachines = e.RoutineTemplate.TagsMachines,
                        IsBodyweight = e.RoutineTemplate.IsBodyweight,
                        RequiresEquipment = e.RoutineTemplate.RequiresEquipment,
                        IsCalisthenic = e.RoutineTemplate.IsCalisthenic,
                        RoutineDays = e.RoutineTemplate.RoutineDays.Select(d => new RoutineDay
                        {
                            Id = d.Id,
                            DayNumber = d.DayNumber,
                            Name = d.Name,
                            Sets = d.Sets,
                            Repetitions = d.Repetitions,
                            Notes = d.Notes,
                            ExerciseId = d.ExerciseId,
                            RoutineTemplateId = d.RoutineTemplateId,
                            IsActive = d.IsActive
                        }).ToList(),
                        RoutineExercises = e.RoutineTemplate.RoutineExercises.Select(rx => new RoutineExercise
                        {
                            Id = rx.Id,
                            ExerciseId = rx.ExerciseId,
                            RoutineTemplateId = rx.RoutineTemplateId,
                            Repetitions = rx.Repetitions,
                            Sets = rx.Sets,
                            IsActive = rx.IsActive
                        }).ToList()
                    }
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RoutineAssigned>> GetAllRoutineAssignedsAsync()
        {
            return await _context.RoutineAssigneds
                .AsNoTracking()
                .Include(e => e.RoutineTemplate)
                .Where(e => e.IsActive)
                .Select(e => new RoutineAssigned
                {
                    Id = e.Id,
                    Comments = e.Comments,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Ip = e.Ip,
                    IsActive = e.IsActive,
                    UserId = e.UserId,
                    RoutineTemplateId = e.RoutineTemplateId,
                    RoutineTemplate = new RoutineTemplate
                    {
                        Id = e.RoutineTemplate.Id,
                        Name = e.RoutineTemplate.Name,
                        Comments = e.RoutineTemplate.Comments,
                        CreatedAt = e.RoutineTemplate.CreatedAt,
                        UpdatedAt = e.RoutineTemplate.UpdatedAt,
                        IsActive = e.RoutineTemplate.IsActive,
                        Premium = e.RoutineTemplate.Premium,
                        IsDefault = e.RoutineTemplate.IsDefault,
                        TagsObjectives = e.RoutineTemplate.TagsObjectives,
                        TagsMachines = e.RoutineTemplate.TagsMachines,
                        IsBodyweight = e.RoutineTemplate.IsBodyweight,
                        RequiresEquipment = e.RoutineTemplate.RequiresEquipment,
                        IsCalisthenic = e.RoutineTemplate.IsCalisthenic
                    }
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateRoutineAssignedAsync(RoutineAssigned entity)
        {
            var existing = await _context.RoutineAssigneds.FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteRoutineAssignedAsync(Guid id)
        {
            var entity = await _context.RoutineAssigneds.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<RoutineAssigned>> FindRoutineAssignedsByFieldsAsync(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(RoutineAssigned), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(RoutineAssigned.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(RoutineAssigned).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<RoutineAssigned, bool>>(predicate, parameter);
            return await _context.RoutineAssigneds
                .AsNoTracking()
                .Include(e => e.RoutineTemplate)
                .Where(lambda)
                .Select(e => new RoutineAssigned
                {
                    Id = e.Id,
                    Comments = e.Comments,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Ip = e.Ip,
                    IsActive = e.IsActive,
                    UserId = e.UserId,
                    RoutineTemplateId = e.RoutineTemplateId,
                    RoutineTemplate = new RoutineTemplate
                    {
                        Id = e.RoutineTemplate.Id,
                        Name = e.RoutineTemplate.Name,
                        Comments = e.RoutineTemplate.Comments,
                        CreatedAt = e.RoutineTemplate.CreatedAt,
                        UpdatedAt = e.RoutineTemplate.UpdatedAt,
                        IsActive = e.RoutineTemplate.IsActive,
                        Premium = e.RoutineTemplate.Premium,
                        IsDefault = e.RoutineTemplate.IsDefault,
                        TagsObjectives = e.RoutineTemplate.TagsObjectives,
                        TagsMachines = e.RoutineTemplate.TagsMachines,
                        IsBodyweight = e.RoutineTemplate.IsBodyweight,
                        RequiresEquipment = e.RoutineTemplate.RequiresEquipment,
                        IsCalisthenic = e.RoutineTemplate.IsCalisthenic
                    }
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RoutineAssigned>> GetActiveByUserAsync(Guid userId)
        {
            return await _context.RoutineAssigneds.AsNoTracking()
                .Where(ra => ra.IsActive && ra.UserId == userId)
                .ToListAsync();
        }
        public async Task<RoutineAssigned?> GetLatestByUserUntilAsync(Guid userId, DateTime untilUtc)
        {
            return await _context.RoutineAssigneds.AsNoTracking()
                .Where(ra => ra.UserId == userId && ra.CreatedAt <= untilUtc)
                .OrderByDescending(ra => ra.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
