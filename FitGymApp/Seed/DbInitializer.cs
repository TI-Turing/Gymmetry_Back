using System.Threading.Tasks;
using FitGymApp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FitGymApp.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(FitGymAppContext context)
        {
            // Aplica migraciones pendientes
            await context.Database.MigrateAsync();
            // Los seeds se aplican automáticamente por HasData en OnModelCreating
        }
    }
}
