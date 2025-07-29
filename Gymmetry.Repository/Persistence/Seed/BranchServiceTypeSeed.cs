using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class BranchServiceTypeSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            var serviceTypes = new[]
            {
                new { Name = "Entrenamiento Personalizado", Description = "Sesiones individuales con entrenador." },
                new { Name = "Clases Grupales", Description = "Clases de yoga, pilates, spinning, etc." },
                new { Name = "Nutrici�n", Description = "Asesor�a y planes nutricionales." },
                new { Name = "Fisioterapia", Description = "Servicios de rehabilitaci�n f�sica." },
                new { Name = "Spa / Masajes", Description = "Masajes y tratamientos de relajaci�n." },
                new { Name = "Evaluaci�n F�sica", Description = "Valoraci�n inicial y seguimiento f�sico." },
                new { Name = "Zona de Cardio", Description = "Acceso a m�quinas de cardio." },
                new { Name = "Zona de Pesas", Description = "Acceso a pesas libres y m�quinas de fuerza." },
                new { Name = "Entrenamiento Funcional", Description = "Circuitos y rutinas funcionales." },
                new { Name = "Otros", Description = "Servicios adicionales seg�n la sede." }
            };

            foreach (var type in serviceTypes)
            {
                if (!context.BranchServiceTypes.Any(x => x.Name == type.Name))
                {
                    context.BranchServiceTypes.Add(new BranchServiceType
                    {
                        Id = Guid.NewGuid(),
                        Name = type.Name,
                        Description = type.Description,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
