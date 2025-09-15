using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class PostShareSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            // Verificar si ya existen datos de PostShare
            if (await context.PostShares.AnyAsync())
            {
                return; // Ya hay datos, no hacer seed
            }

            // Obtener algunos posts y usuarios existentes para crear shares de prueba
            var posts = await context.Posts.Where(p => p.IsActive && !p.IsDeleted).Take(3).ToListAsync();
            var users = await context.Users.Where(u => u.IsActive == true).Take(5).ToListAsync();

            if (!posts.Any() || !users.Any())
            {
                return; // No hay posts o usuarios para crear shares de prueba
            }

            var postShares = new List<PostShare>();
            var random = new Random();

            // Crear shares de ejemplo
            foreach (var post in posts)
            {
                foreach (var user in users.Take(3)) // Solo los primeros 3 usuarios
                {
                    // Share externo por WhatsApp
                    postShares.Add(new PostShare
                    {
                        Id = Guid.NewGuid(),
                        PostId = post.Id,
                        SharedBy = user.Id,
                        SharedWith = null,
                        ShareType = "External",
                        Platform = "WhatsApp",
                        Metadata = $"{{\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"}}",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 1440)), // Últimas 24 horas
                        IsActive = true,
                        Ip = $"192.168.1.{random.Next(1, 255)}"
                    });

                    // Share interno en la app (si hay otro usuario disponible)
                    var otherUser = users.FirstOrDefault(u => u.Id != user.Id);
                    if (otherUser != null)
                    {
                        postShares.Add(new PostShare
                        {
                            Id = Guid.NewGuid(),
                            PostId = post.Id,
                            SharedBy = user.Id,
                            SharedWith = otherUser.Id,
                            ShareType = "Internal",
                            Platform = "App",
                            Metadata = $"{{\"internal_note\":\"Shared within app\",\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"}}",
                            CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 720)), // Últimas 12 horas
                            IsActive = true,
                            Ip = $"10.0.0.{random.Next(1, 255)}"
                        });
                    }

                    // Ocasionalmente agregar shares en otras plataformas
                    if (random.NextDouble() < 0.5) // 50% probabilidad
                    {
                        var platforms = new[] { "Instagram", "Facebook", "Twitter", "Email" };
                        var selectedPlatform = platforms[random.Next(platforms.Length)];
                        
                        postShares.Add(new PostShare
                        {
                            Id = Guid.NewGuid(),
                            PostId = post.Id,
                            SharedBy = user.Id,
                            SharedWith = null,
                            ShareType = "External",
                            Platform = selectedPlatform,
                            Metadata = $"{{\"platform\":\"{selectedPlatform}\",\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\"}}",
                            CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1, 2880)), // Últimas 48 horas
                            IsActive = true,
                            Ip = $"172.16.{random.Next(1, 255)}.{random.Next(1, 255)}"
                        });
                    }
                }
            }

            // Agregar shares a la base de datos
            if (postShares.Any())
            {
                context.PostShares.AddRange(postShares);
                await context.SaveChangesAsync();
            }
        }
    }
}