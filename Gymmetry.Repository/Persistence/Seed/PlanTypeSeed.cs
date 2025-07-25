using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class PlanTypeSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            if (!context.PlanTypes.Any(x => x.Name == "Plan Gratuito"))
            {
                context.PlanTypes.Add(new PlanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Plan Gratuito",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Description = "Ideal para enganchar usuarios y mostrar el valor de Gymmetry.\n\nIncluye:\n- Acceso a rutinas b�sicas predeterminadas.\n- Registro de progreso f�sico (peso, medidas).\n- Visualizaci�n de rutinas asignadas por su gimnasio.\n- Seguimiento de asistencia y entrenamientos.\n- Acceso limitado al feed/red social de la app.\n- Notificaciones b�sicas (por ejemplo: recordatorio de rutina del d�a)."
                });
            }

            if (!context.PlanTypes.Any(x => x.Name == "Plan B�sico"))
            {
                context.PlanTypes.Add(new PlanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Plan B�sico",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Price = 5000,
                    Description = "Para usuarios que quieren m�s personalizaci�n sin ir al extremo.\n\nIncluye todo lo anterior, m�s:\n- Rutinas personalizadas seg�n objetivos (ganancia muscular, p�rdida de peso, etc.).\n- Historial detallado de entrenamientos.\n- Acceso a planes de dieta b�sicos (dise�ados por IA o plantillas).\n- Soporte por chat b�sico con entrenadores del gimnasio (si el gimnasio tiene esa opci�n).\n- Estad�sticas semanales de progreso."
                });
            }

            if (!context.PlanTypes.Any(x => x.Name == "Plan Premium"))
            {
                context.PlanTypes.Add(new PlanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Plan Premium",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Price = 15000,
                    Description = "Para usuarios comprometidos con su salud y que quieren lo mejor.\n\nIncluye todo lo anterior, m�s:\n- Chat con nutricionistas o entrenadores certificados (seg�n disponibilidad).\n- Planes de dieta personalizados.\n- Videos de cada ejercicio en su rutina.\n- Correcci�n de t�cnica por IA (subiendo un video corto o an�lisis del movimiento).\n- Comunidad exclusiva con eventos, retos mensuales y premios.\n- Sincronizaci�n con wearables (smartwatch, banda card�aca)."
                });
            }

            if (!context.PlanTypes.Any(x => x.Name == "Plan Familiar o Duo"))
            {
                context.PlanTypes.Add(new PlanType
                {
                    Id = Guid.NewGuid(),
                    Name = "Plan Familiar o Duo",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Price = 25000,
                    Description = "Pensado para parejas o familias que entrenan juntos.\n\nIncluye todo lo del Premium, para 2 o m�s usuarios.\n- Descuentos para familiares adicionales.\n- Retos familiares o entre amigos.\n- Compartir progreso en grupo."
                });
            }

            await context.SaveChangesAsync();
        }
    }
}