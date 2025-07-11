using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FitGymApp.Domain.Models;
using FitGymApp.Application.Services;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Repository.Services;
using FitGymApp.Repository.Services.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuración de cadena de conexión
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? configuration["ConnectionStrings:DefaultConnection"]
    ?? configuration["ConnectionStrings:FitGymApp"];

// Registrar DbContext
builder.Services.AddDbContext<FitGymAppContext>(options =>
    options.UseSqlServer(connectionString));

// Inyección de dependencias de repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ILogErrorRepository, LogErrorRepository>();
builder.Services.AddScoped<ILogLoginRepository, LogLoginRepository>();
builder.Services.AddScoped<ILogChangeRepository, LogChangeRepository>();

// Inyección de dependencias de servicios de aplicación
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILogErrorService, LogErrorService>();
builder.Services.AddScoped<ILogLoginService, LogLoginService>();
builder.Services.AddScoped<ILogChangeService, LogChangeService>();

// Aplicar migraciones y ejecutar seeds (opcional, solo en desarrollo o si es seguro)
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitGymAppContext>();
    db.Database.Migrate(); // Aplica migraciones pendientes
    // TODO: Llamar a método de seeders aquí si lo implementas
}

builder.Build().Run();
