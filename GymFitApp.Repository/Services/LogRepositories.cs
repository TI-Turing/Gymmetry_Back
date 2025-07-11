using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;
using System;

namespace FitGymApp.Repository.Services
{
    public class LogErrorRepository : ILogErrorRepository
    {
        private readonly FitGymApp.Domain.Models.FitGymAppContext _context;
        public LogErrorRepository(FitGymApp.Domain.Models.FitGymAppContext context) { _context = context; }
        public bool Add(LogError log)
        {
            _context.LogErrors.Add(log);
            return _context.SaveChanges() > 0;
        }
    }
    public class LogLoginRepository : ILogLoginRepository
    {
        private readonly FitGymApp.Domain.Models.FitGymAppContext _context;
        public LogLoginRepository(FitGymApp.Domain.Models.FitGymAppContext context) { _context = context; }
        public bool Add(LogLogin log)
        {
            _context.LogLogins.Add(log);
            return _context.SaveChanges() > 0;
        }
    }
    public class LogChangeRepository : ILogChangeRepository
    {
        private readonly FitGymApp.Domain.Models.FitGymAppContext _context;
        public LogChangeRepository(FitGymApp.Domain.Models.FitGymAppContext context) { _context = context; }
        public bool Add(LogChange log)
        {
            _context.LogChanges.Add(log);
            return _context.SaveChanges() > 0;
        }
    }
}
