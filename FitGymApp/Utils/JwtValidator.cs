using Microsoft.AspNetCore.Http;
using System;
using FitGymApp.Application.Services;

namespace FitGymApp.Utils
{
    public static class JwtValidator
    {
        public static bool ValidateJwt(HttpRequest req, out string? error)
        {
            error = null;
            if (!req.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            {
                error = "Token no proporcionado.";
                return false;
            }
            var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            var principal = JwtTokenGenerator.ValidateToken(token);
            if (principal == null)
            {
                error = "Token inválido o expirado.";
                return false;
            }
            return true;
        }
    }
}
