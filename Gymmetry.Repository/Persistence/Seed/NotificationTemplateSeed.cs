using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Persistence.Seed
{
    public static class NotificationTemplateSeed
    {
        public static async Task SeedAsync(GymmetryContext context)
        {
            if (await context.NotificationTemplates.AnyAsync()) return;

            var templates = new[]
            {
                // Fitness Templates
                new NotificationTemplate
                {
                    TemplateKey = "workout_reminder",
                    NotificationType = "fitness",
                    Priority = "normal",
                    Subject = "�Hora de entrenar! ??",
                    BodyTemplate = "�Es hora de tu entrenamiento! No dejes que el gimnasio te espere.",
                    EmailTemplate = "<h2>�Hora de entrenar! ??</h2><p>Es hora de tu entrenamiento programado. �No dejes que el gimnasio te espere!</p>",
                    SmsTemplate = "�Hora de entrenar! ?? Tu entrenamiento te espera.",
                    AllowedChannels = "push,app"
                },
                new NotificationTemplate
                {
                    TemplateKey = "streak_achievement",
                    NotificationType = "fitness",
                    Priority = "normal",
                    Subject = "�Racha de {days} d�as! ??",
                    BodyTemplate = "�Incre�ble! Has mantenido tu racha de entrenamiento por {days} d�as consecutivos. �Sigue as�!",
                    EmailTemplate = "<h2>�Racha de {days} d�as! ??</h2><p>�Incre�ble! Has mantenido tu racha de entrenamiento por {days} d�as consecutivos. �Sigue as�!</p>",
                    AllowedChannels = "push,app,email"
                },
                new NotificationTemplate
                {
                    TemplateKey = "routine_completed",
                    NotificationType = "fitness",
                    Priority = "low",
                    Subject = "�Rutina completada! ?",
                    BodyTemplate = "�Felicitaciones! Has completado tu rutina de {routineName}. Tiempo total: {duration} minutos.",
                    AllowedChannels = "push,app"
                },

                // Social Templates
                new NotificationTemplate
                {
                    TemplateKey = "new_follower",
                    NotificationType = "social",
                    Priority = "low",
                    Subject = "Nuevo seguidor",
                    BodyTemplate = "{username} comenz� a seguirte",
                    AllowedChannels = "push,app"
                },
                new NotificationTemplate
                {
                    TemplateKey = "post_liked",
                    NotificationType = "social",
                    Priority = "low",
                    Subject = "Le gust� tu publicaci�n",
                    BodyTemplate = "A {username} le gust� tu publicaci�n",
                    AllowedChannels = "push,app"
                },
                new NotificationTemplate
                {
                    TemplateKey = "new_comment",
                    NotificationType = "social",
                    Priority = "normal",
                    Subject = "Nuevo comentario",
                    BodyTemplate = "{username} coment� tu publicaci�n: \"{comment}\"",
                    AllowedChannels = "push,app"
                },

                // Billing Templates
                new NotificationTemplate
                {
                    TemplateKey = "price_increase",
                    NotificationType = "billing",
                    Priority = "high",
                    Subject = "?? Cambio de precio del plan {planName}",
                    BodyTemplate = "Te informamos que el precio del plan {planName} cambiar� a ${newPrice} a partir del {effectiveDate}.",
                    EmailTemplate = "<h2>?? Cambio de precio</h2><p>Te informamos que el precio del plan <strong>{planName}</strong> cambiar� a <strong>${newPrice}</strong> a partir del <strong>{effectiveDate}</strong>.</p>",
                    SmsTemplate = "AVISO: Plan {planName} cambiar� a ${newPrice} desde {effectiveDate}",
                    RequiresEmail = true,
                    AllowedChannels = "push,app,email,sms"
                },
                new NotificationTemplate
                {
                    TemplateKey = "payment_failed",
                    NotificationType = "billing",
                    Priority = "critical",
                    Subject = "? Error en el pago - Acci�n requerida",
                    BodyTemplate = "No pudimos procesar tu pago de ${amount}. Por favor, actualiza tu m�todo de pago para continuar.",
                    EmailTemplate = "<h2>? Error en el pago</h2><p>No pudimos procesar tu pago de <strong>${amount}</strong>. Por favor, actualiza tu m�todo de pago para continuar con tu suscripci�n.</p>",
                    SmsTemplate = "Error en pago de ${amount}. Actualiza m�todo de pago urgente.",
                    RequiresEmail = true,
                    RequiresSms = true,
                    AllowedChannels = "push,app,email,sms"
                },
                new NotificationTemplate
                {
                    TemplateKey = "subscription_ending",
                    NotificationType = "billing",
                    Priority = "high",
                    Subject = "? Tu suscripci�n vence en {days} d�as",
                    BodyTemplate = "Tu suscripci�n del plan {planName} vence el {expirationDate}. �Renueva para seguir disfrutando!",
                    EmailTemplate = "<h2>? Renovaci�n pendiente</h2><p>Tu suscripci�n del plan <strong>{planName}</strong> vence el <strong>{expirationDate}</strong>. �Renueva para seguir disfrutando de todos los beneficios!</p>",
                    AllowedChannels = "push,app,email"
                },

                // Security Templates
                new NotificationTemplate
                {
                    TemplateKey = "suspicious_login",
                    NotificationType = "security",
                    Priority = "critical",
                    Subject = "?? Acceso desde nueva ubicaci�n",
                    BodyTemplate = "Detectamos un acceso a tu cuenta desde {location} el {timestamp}. Si no fuiste t�, cambia tu contrase�a inmediatamente.",
                    EmailTemplate = "<h2>?? Acceso desde nueva ubicaci�n</h2><p>Detectamos un acceso a tu cuenta desde <strong>{location}</strong> el <strong>{timestamp}</strong>. Si no fuiste t�, cambia tu contrase�a inmediatamente.</p>",
                    SmsTemplate = "ALERTA: Acceso a tu cuenta desde {location}. Si no fuiste t�, cambia tu contrase�a.",
                    RequiresEmail = true,
                    RequiresSms = true,
                    AllowedChannels = "push,app,email,sms"
                },
                new NotificationTemplate
                {
                    TemplateKey = "password_changed",
                    NotificationType = "security",
                    Priority = "high",
                    Subject = "? Contrase�a actualizada exitosamente",
                    BodyTemplate = "Tu contrase�a fue cambiada exitosamente el {timestamp}. Si no realizaste este cambio, contacta soporte inmediatamente.",
                    EmailTemplate = "<h2>? Contrase�a actualizada</h2><p>Tu contrase�a fue cambiada exitosamente el <strong>{timestamp}</strong>. Si no realizaste este cambio, contacta soporte inmediatamente.</p>",
                    AllowedChannels = "push,app,email"
                },
                new NotificationTemplate
                {
                    TemplateKey = "account_locked",
                    NotificationType = "security",
                    Priority = "critical",
                    Subject = "?? Cuenta bloqueada por seguridad",
                    BodyTemplate = "Tu cuenta ha sido bloqueada temporalmente por intentos de acceso sospechosos. Contacta soporte para desbloqueabrla.",
                    EmailTemplate = "<h2>?? Cuenta bloqueada</h2><p>Tu cuenta ha sido bloqueada temporalmente por intentos de acceso sospechosos. Contacta nuestro equipo de soporte para desbloquearla.</p>",
                    SmsTemplate = "URGENTE: Cuenta bloqueada por seguridad. Contacta soporte para desbloquear.",
                    RequiresEmail = true,
                    RequiresSms = true,
                    AllowedChannels = "push,app,email,sms"
                },

                // Moderation Templates
                new NotificationTemplate
                {
                    TemplateKey = "content_reported",
                    NotificationType = "moderation",
                    Priority = "high",
                    Subject = "?? Tu contenido fue reportado",
                    BodyTemplate = "Tu {contentType} fue reportado por {reason}. Nuestro equipo lo revisar�. Si viola nuestras normas, puede ser removido.",
                    EmailTemplate = "<h2>?? Contenido reportado</h2><p>Tu <strong>{contentType}</strong> fue reportado por <strong>{reason}</strong>. Nuestro equipo lo revisar�. Si viola nuestras normas, puede ser removido.</p>",
                    AllowedChannels = "push,app,email"
                },
                new NotificationTemplate
                {
                    TemplateKey = "account_warning",
                    NotificationType = "moderation",
                    Priority = "critical",
                    Subject = "?? Advertencia por violaci�n de normas",
                    BodyTemplate = "Has recibido una advertencia por {reason}. Duraci�n: {duration}. M�s infracciones pueden resultar en suspensi�n permanente.",
                    EmailTemplate = "<h2>?? Advertencia oficial</h2><p>Has recibido una advertencia por <strong>{reason}</strong>. Duraci�n: <strong>{duration}</strong>. M�s infracciones pueden resultar en suspensi�n permanente.</p>",
                    SmsTemplate = "ADVERTENCIA: {reason}. Duraci�n: {duration}. Evita m�s infracciones.",
                    RequiresEmail = true,
                    AllowedChannels = "push,app,email,sms"
                },
                new NotificationTemplate
                {
                    TemplateKey = "account_suspended",
                    NotificationType = "moderation",
                    Priority = "critical",
                    Subject = "? Cuenta suspendida temporalmente",
                    BodyTemplate = "Tu cuenta ha sido suspendida por {reason} hasta el {until}. Para apelar, contacta soporte con el ID: {appealId}.",
                    EmailTemplate = "<h2>? Cuenta suspendida</h2><p>Tu cuenta ha sido suspendida por <strong>{reason}</strong> hasta el <strong>{until}</strong>. Para apelar esta decisi�n, contacta soporte con el ID: <strong>{appealId}</strong>.</p>",
                    SmsTemplate = "SUSPENSI�N: {reason} hasta {until}. ID apelaci�n: {appealId}",
                    RequiresEmail = true,
                    RequiresSms = true,
                    AllowedChannels = "push,app,email,sms"
                }
            };

            await context.NotificationTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
        }
    }
}