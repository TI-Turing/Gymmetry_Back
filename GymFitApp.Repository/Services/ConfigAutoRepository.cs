using System.Linq;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitGymApp.Repository.Services
{
    public class ConfigAutoRepository : IConfigAutoRepository
    {
        private readonly FitGymAppContext _context;
        public ConfigAutoRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public async Task UpdateUsdPricesAsync(decimal usdToCopRate)
        {
            var plans = await _context.GymPlanSelectedTypes.Where(p => p.Price != null).ToListAsync();
            foreach (var plan in plans)
            {
                plan.UsdPrice = plan.Price / usdToCopRate;
            }
            await _context.SaveChangesAsync();
        }
    }
}
