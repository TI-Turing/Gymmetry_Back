using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class GymTypeSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            var gymTypes = new[]
            {
                new { Name = "Gimnasio Convencional", Description = "Gimnasios de uso general que ofrecen m�quinas de pesas, cardio y clases grupales. Ideales para principiantes o personas que buscan mantenerse en forma." },
                new { Name = "Gimnasio Boutique", Description = "Peque�os, modernos y especializados en una experiencia premium. Ofrecen clases enfocadas como HIIT, spinning, pilates o yoga, con atenci�n personalizada." },
                new { Name = "Gimnasio Funcional / Cuerpo Libre", Description = "Enfocados en ejercicios de fuerza y movilidad usando el peso corporal, TRX o circuitos funcionales. Ideales para quienes evitan m�quinas tradicionales." },
                new { Name = "Gimnasio Hardcore / Culturismo", Description = "Espacios r�sticos y orientados al entrenamiento intenso con pesas libres y m�quinas b�sicas. Enfocados en desarrollo muscular y fuerza." },
                new { Name = "Gimnasio de Artes Marciales / Combate", Description = "Especializados en boxeo, MMA, jiu-jitsu, muay thai, entre otros. Equipados con sacos, tatamis y rings para entrenamientos de contacto y defensa personal." },
                new { Name = "Gimnasio de CrossFit", Description = "Centros con enfoque en entrenamientos funcionales de alta intensidad (WODs). Equipados con barras ol�mpicas, cajones, kettlebells y �reas abiertas." },
                new { Name = "Gimnasio de Rehabilitaci�n / Fisioterapia", Description = "Dise�ados para recuperaci�n f�sica, con equipos suaves y supervisi�n m�dica o fisioterap�utica. Enfocados en personas con lesiones o movilidad reducida." },
                new { Name = "Gimnasio Infantil / Familiar", Description = "Espacios seguros con actividades f�sicas l�dicas para ni�os. Promueven el desarrollo psicomotor y h�bitos saludables desde temprana edad." },
                new { Name = "Gimnasio para Adultos Mayores", Description = "Adaptados con equipos de bajo impacto y supervisi�n constante. Enfocados en movilidad, fuerza funcional y salud cardiovascular para personas mayores." },
                new { Name = "Gimnasio Virtual / Online", Description = "Servicios 100% digitales con entrenamientos por videollamada, apps o plataformas. Ideales para quienes prefieren entrenar desde casa." }
            };

            foreach (var type in gymTypes)
            {
                if (!context.GymTypes.Any(x => x.Name == type.Name))
                {
                    context.GymTypes.Add(new GymType
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
