using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class AccessMethodTypeSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            var accessMethods = new[]
            {
                new { Name = "C�digo QR din�mico", Description = "Acceso mediante c�digo QR generado por la app, que se escanea al ingresar." },
                new { Name = "C�digo de barras o PIN", Description = "Acceso mediante un c�digo de barras impreso o un PIN digital mostrado en la app." },
                new { Name = "Tarjeta RFID o NFC", Description = "Acceso mediante tarjeta, manilla o llavero con chip RFID/NFC, escaneado por un lector." },
                new { Name = "Reconocimiento facial", Description = "Acceso mediante reconocimiento facial usando c�mara con IA." },
                new { Name = "Huella dactilar", Description = "Acceso mediante lectura de huella dactilar registrada previamente." },
                new { Name = "Validaci�n manual por recepci�n", Description = "Acceso validado por personal del gimnasio en la recepci�n." },
                new { Name = "Ingreso por geolocalizaci�n", Description = "Acceso validado por ubicaci�n f�sica del usuario cerca del gimnasio." },
                new { Name = "Sin m�todo de acceso", Description = "Acceso sin control de entrada o personalizado." }
            };


            foreach (var method in accessMethods)
            {
                if (!context.AccessMethodTypes.Any(x => x.Name == method.Name))
                {
                    context.AccessMethodTypes.Add(new AccessMethodType
                    {
                        Id = Guid.NewGuid(),
                        Name = method.Name,
                        Description = method.Description,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}