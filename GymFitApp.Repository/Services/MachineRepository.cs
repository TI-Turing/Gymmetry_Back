using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class MachineRepository : IMachineRepository
    {
        private readonly FitGymAppContext _context;
        public MachineRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Machine CreateMachine(Machine machine)
        {
            machine.Id = Guid.NewGuid();
            machine.CreatedAt = DateTime.UtcNow;
            machine.IsActive = true;
            _context.Machines.Add(machine);
            _context.SaveChanges();
            return machine;
        }

        public Machine GetMachineById(Guid id)
        {
            return _context.Machines.FirstOrDefault(m => m.Id == id && m.IsActive);
        }

        public IEnumerable<Machine> GetAllMachines()
        {
            return _context.Machines.Where(m => m.IsActive).ToList();
        }

        public bool UpdateMachine(Machine machine)
        {
            var existing = _context.Machines.FirstOrDefault(m => m.Id == machine.Id && m.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(machine);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteMachine(Guid id)
        {
            var entity = _context.Machines.FirstOrDefault(m => m.Id == id && m.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Machine> FindMachinesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Machine), "m");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Machine.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Machine).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Machine, bool>>(predicate, parameter);
            return _context.Machines.Where(lambda).ToList();
        }
    }
}
