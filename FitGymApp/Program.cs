using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Gymmetry.Domain.Models;
using Gymmetry.Application.Services;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Repository.Services;
using Gymmetry.Repository.Services.Interfaces;
using Gymmetry.Repository.Persistence.Seed;
using Gymmetry.Repository.Services.Cache;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Payments;
using Gymmetry.Domain.Options;
using FitGymApp.Hubs;
using Azure.Storage.Blobs;
using FitGymApp.Middleware;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Middleware CORS + API Key (query ?code=...)
// (Registrar una sola vez cada middleware)
builder.Services.AddSingleton<IFunctionsWorkerMiddleware, CorsMiddleware>();
builder.Services.AddSingleton<IFunctionsWorkerMiddleware, ApiKeyMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuración de cadena de conexión
var configuration = builder.Configuration;
string? ResolveConn(params string[] keys)
{
    foreach (var k in keys)
    {
        var v = configuration[k];
        if (!string.IsNullOrWhiteSpace(v)) return v;
    }
    return null;
}
var connectionString = ResolveConn(
    "Values:ConnectionStrings:DefaultConnection",
    "Values:ConnectionStrings:Gymmetry",
    "ConnectionStrings:DefaultConnection",
    "ConnectionStrings:Gymmetry",
    configuration.GetConnectionString("DefaultConnection") ?? string.Empty
) ?? "";

// Registrar DbContext
builder.Services.AddDbContext<GymmetryContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<AutoMapperProfile>(); });

// OAuth client credentials options
builder.Services.Configure<OAuthOptions>(opts =>
{
    var section = configuration.GetSection("OAuth");
    section.Bind(opts);
});

// Azure Blob Storage
builder.Services.AddSingleton(sp => new BlobServiceClient(configuration["AzureStorage:ConnectionString"] ?? configuration["Values:AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true"));
builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

// Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>(sp => new UserRepository(
    sp.GetRequiredService<GymmetryContext>(),
    sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
    sp.GetRequiredService<IRedisCacheService>()));
builder.Services.AddScoped<IFeedRepository, FeedRepository>(sp => new FeedRepository(
    sp.GetRequiredService<GymmetryContext>(),
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<IRedisCacheService>(),
    sp.GetRequiredService<ILogger<FeedRepository>>()));
builder.Services.AddScoped<IAccessMethodTypeRepository, AccessMethodTypeRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ILogErrorRepository, LogErrorRepository>();
builder.Services.AddScoped<ILogLoginRepository, LogLoginRepository>();
builder.Services.AddScoped<ILogChangeRepository, LogChangeRepository>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICategoryExerciseRepository, CategoryExerciseRepository>();
builder.Services.AddScoped<IConfigAutoRepository, ConfigAutoRepository>();
builder.Services.AddScoped<IDailyExerciseHistoryRepository, DailyExerciseHistoryRepository>();
builder.Services.AddScoped<IDailyExerciseRepository, DailyExerciseRepository>();
builder.Services.AddScoped<IDailyHistoryRepository, DailyHistoryRepository>();
builder.Services.AddScoped<IDailyRepository, DailyRepository>();
builder.Services.AddScoped<IDietRepository, DietRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
builder.Services.AddScoped<IEmployeeRegisterDailyRepository, EmployeeRegisterDailyRepository>();
builder.Services.AddScoped<IEmployeeTypeRepository, EmployeeTypeRepository>();
builder.Services.AddScoped<IEmployeeUserRepository, EmployeeUserRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IFitUserRepository, FitUserRepository>();
builder.Services.AddScoped<IGymPlanSelectedModuleRepository, GymPlanSelectedModuleRepository>();
builder.Services.AddScoped<IGymPlanSelectedRepository, GymPlanSelectedRepository>();
builder.Services.AddScoped<IGymPlanSelectedTypeRepository, GymPlanSelectedTypeRepository>();
builder.Services.AddScoped<IGymRepository, GymRepository>();
builder.Services.AddScoped<IGymTypeRepository, GymTypeRepository>();
builder.Services.AddScoped<IJourneyEmployeeRepository, JourneyEmployeeRepository>();
builder.Services.AddScoped<ILogUninstallRepository, LogUninstallRepository>();
builder.Services.AddScoped<IMachineCategoryRepository, MachineCategoryRepository>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<INotificationOptionRepository, NotificationOptionRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserOtpRepository, UserOtpRepository>();
builder.Services.AddScoped<IPhysicalAssessmentRepository, PhysicalAssessmentRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanTypeRepository, PlanTypeRepository>();
builder.Services.AddScoped<IRoutineAssignedRepository, RoutineAssignedRepository>();
builder.Services.AddScoped<IRoutineDayRepository, RoutineDayRepository>();
builder.Services.AddScoped<IRoutineExerciseRepository, RoutineExerciseRepository>();
builder.Services.AddScoped<IRoutineTemplateRepository, RoutineTemplateRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<ISubModuleRepository, SubModuleRepository>();
builder.Services.AddScoped<IUninstallOptionRepository, UninstallOptionRepository>();
builder.Services.AddScoped<IUserExerciseMaxRepository, UserExerciseMaxRepository>();
builder.Services.AddScoped<IUserTypeRepository, UserTypeRepository>();
builder.Services.AddScoped<IVerificationTypeRepository, VerificationTypeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IGymImageRepository, GymImageRepository>();
builder.Services.AddScoped<IPaymentIntentRepository, PaymentIntentRepository>();
builder.Services.AddScoped<IReportContentRepository, ReportContentRepository>();
builder.Services.AddScoped<IUserBlockRepository, UserBlockRepository>();
builder.Services.AddScoped<IContentModerationRepository, ContentModerationRepository>();

// PaymentsOptions
int resolveGatewayProvider()
{
    var gp = configuration["Payments:GatewayProvider"] ?? configuration["Values:Payments:GatewayProvider"];
    return int.TryParse(gp, out var n) ? n : 3;
}
string? resolve(string key) => configuration[$"Payments:{key}"] ?? configuration[$"Values:Payments:{key}"];

builder.Services.Configure<PaymentsOptions>(opts =>
{
    opts.GatewayProvider = resolveGatewayProvider();
    opts.BaseSuccessUrl = resolve("BaseSuccessUrl");
    opts.BaseFailureUrl = resolve("BaseFailureUrl");
    opts.BasePendingUrl = resolve("BasePendingUrl");
});

builder.Services.AddHttpClient<MercadoPagoGatewayRepository>();
builder.Services.AddScoped<IPaymentGatewayRepository, MercadoPagoGatewayRepository>();
builder.Services.AddScoped<IPaymentIntentService, PaymentIntentService>();

var providerValue = resolveGatewayProvider();
if (providerValue == 1)
    builder.Services.AddScoped<IPaymentGatewayService, PayUPaymentGatewayService>();
else if (providerValue == 2)
    builder.Services.AddScoped<IPaymentGatewayService, WompiPaymentGateway>();
else if (providerValue == 3)
    builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoPaymentGatewayService>();
else if (providerValue == 4)
    builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoPaymentGatewayService>();

// Servicios de aplicación
builder.Services.AddScoped<IAccessMethodTypeService, AccessMethodTypeService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAppStateService, AppStateService>(); // NEW: AppState aggregator service
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IConfigAutoService, ConfigAutoService>();
builder.Services.AddScoped<ILogErrorService, LogErrorService>();
builder.Services.AddScoped<ILogLoginService, LogLoginService>();
builder.Services.AddScoped<ILogChangeService, LogChangeService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IBranchService, Gymmetry.Application.Services.BranchService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryExerciseService, CategoryExerciseService>();
builder.Services.AddScoped<IDailyExerciseHistoryService, DailyExerciseHistoryService>();
builder.Services.AddScoped<IDailyExerciseService, DailyExerciseService>();
builder.Services.AddScoped<IDailyHistoryService, DailyHistoryService>();
builder.Services.AddScoped<IDailyService, DailyService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmployeeRegisterDailyService, EmployeeRegisterDailyService>();
builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
builder.Services.AddScoped<IEmployeeUserService, EmployeeUserService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IMediaProcessingService, MediaProcessingService>(); // NEW: Media processing for multimedia feeds
builder.Services.AddScoped<IFitUserService, FitUserService>();
builder.Services.AddScoped<IGymPlanSelectedModuleService, GymPlanSelectedModuleService>();
builder.Services.AddScoped<IGymPlanSelectedService, GymPlanSelectedService>();
builder.Services.AddScoped<IGymPlanSelectedTypeService, GymPlanSelectedTypeService>();
builder.Services.AddScoped<IGymService, GymService>();
builder.Services.AddScoped<IGymTypeService, GymTypeService>();
builder.Services.AddScoped<IJourneyEmployeeService, JourneyEmployeeService>();
builder.Services.AddScoped<ILogUninstallService, LogUninstallService>();
builder.Services.AddScoped<IMachineCategoryService, MachineCategoryService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<INotificationOptionService, NotificationOptionService>();
builder.Services.AddScoped<IUserOtpService, UserOtpService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPhysicalAssessmentService, PhysicalAssessmentService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPlanTypeService, PlanTypeService>();
builder.Services.AddScoped<IProgressReportService, ProgressReportService>();
builder.Services.AddScoped<IRoutineAssignedService, RoutineAssignedService>();
builder.Services.AddScoped<IRoutineDayService, RoutineDayService>();
builder.Services.AddScoped<IRoutineExerciseService, RoutineExerciseService>();
builder.Services.AddScoped<IRoutineTemplateService, RoutineTemplateService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ISubModuleService, SubModuleService>();
builder.Services.AddScoped<IUninstallOptionService, UninstallOptionService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<IReportContentService, ReportContentService>();
builder.Services.AddScoped<IUserBlockService, UserBlockService>();
builder.Services.AddScoped<IContentModerationService, ContentModerationService>();

builder.Services.AddHttpClient<Gymmetry.Application.Services.ConfigAutoService>();
builder.Services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(builder.Configuration);

builder.Services.AddScoped<IClientCredentialsService, ClientCredentialsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Redis
var redisConnectionString = configuration["Redis:ConnectionString"] ?? configuration["Values:Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IRedisCacheService>(sp => new RedisCacheService(
    redisConnectionString,
    sp.GetRequiredService<ILogger<RedisCacheService>>()));

// Notificaciones
builder.Services.AddScoped<IReportContentRealtimeService, ReportContentRealtimeService>();
builder.Services.AddScoped<IReportContentEvidenceService, ReportContentEvidenceService>();
builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
builder.Services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();
builder.Services.AddScoped<IUnifiedNotificationService, UnifiedNotificationService>();
builder.Services.AddScoped<INotificationDeliveryService, NotificationDeliveryService>();

// PostShare
builder.Services.AddScoped<IPostShareRepository, PostShareRepository>();
builder.Services.AddScoped<IPostShareService, PostShareService>();

// External settings
builder.Services.Configure<MailGunSettings>(configuration.GetSection("MailGun"));
builder.Services.Configure<TwilioSettings>(configuration.GetSection("Twilio"));
builder.Services.Configure<ExpoPushSettings>(configuration.GetSection("ExpoPush"));

builder.Services.AddHttpClient<INotificationDeliveryService, NotificationDeliveryService>();

// Seeds & migraciones
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymmetryContext>();
    try
    {
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any()) db.Database.Migrate();
        DbInitializer.SeedAsync(db).GetAwaiter().GetResult();
        var configAutoService = scope.ServiceProvider.GetRequiredService<IConfigAutoService>();
        configAutoService.UpdateUsdPricesFromExchangeAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Error durante la inicialización de la base de datos");
    }
}

var built = builder.Build();
built.Run();
