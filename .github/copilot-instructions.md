# Gymmetry_Back – Instrucciones para agentes de IA

Estas pautas te ayudarán a contribuir con cambios útiles y consistentes en este backend (.NET 9 + Azure Functions + SQL Server) sin perder tiempo. Sé específico, reutiliza servicios existentes y respeta los patrones del repo.

## Arquitectura y capas
- Entradas: Azure Functions HTTP/Timer en `FitGymApp/Functions/**`. Cada carpeta agrupa un módulo (por ejemplo, `UserFunctions`, `MachineFunction`, `Payments`).
- Aplicación/Negocio: Servicios en `Gymmetry.Application/Services/**` con interfaces en `.../Interfaces`.
- Persistencia: Repositorios en `Gymmetry.Repository/**` (EF Core vía `GymmetryContext`). Cache con `IRedisCacheService`.
- Dominio/DTOs: `Gymmetry.Domain/{Models,DTO,Enums}`.
- Utilidades: `Gymmetry.Utils` (p. ej., `FunctionResponseHelper`, validadores JWT).
- DI, EF, AutoMapper y opciones están centralizados en `FitGymApp/Program.cs`.

## Patrones de Functions (HTTP)
- Firma típica: un método por endpoint con `[Function("Name")]` y `[HttpTrigger(..., Route = "...")]`.
- Autenticación: valida siempre JWT al inicio con `JwtValidator.ValidateJwt(req, out var error, out var userId)`. Si falla, devuelve 401 con `ApiResponse<T>`.
- Respuesta estandarizada: usa `ApiResponse<T>` y `FunctionResponseHelper.CreateResponseAsync` para escribir JSON y `StatusCodes` correctos.
- Ejemplo real: ver `FitGymApp/Functions/UserFunctions/GetUserFunction.cs` (endpoints `User_GetUserByIdFunction`, `User_GetAllUsersFunction`, etc.).
- Registro: usa `executionContext.GetLogger("<FunctionName>")` y `FunctionResponseHelper.LogError(...)` en excepciones.
- Creacion de entidades nuevas: siempre usar Id tipo GUID (no int). Evitar usar IDs secuenciales o autoincrementales. Cada entidaded debe llevar estos campos siempre: `Id (GUID)`, `CreatedAt (DateTime)`, `UpdatedAt (DateTime?)`, `IsActive (bool)`, `Ip (string)`.

## Inyección de dependencias y configuración
- Registra servicios, repositorios y opciones en `FitGymApp/Program.cs`. Busca antes de crear servicios nuevos; probablemente ya existan interfaces similares.
- Conexiones y opciones se resuelven con prioridad `Values:<Section>` (local.settings.json) y luego `<Section>`. Ej.: conexión SQL y `Payments:*`.
- AutoMapper: agrega mapeos en `AutoMapperProfile` (ya registrado en Program).

## Acceso a datos y seeds
- EF Core configurado con SQL Server en `Program.cs`. En el arranque se aplican migraciones pendientes y `DbInitializer.SeedAsync(db)`.
- Al crear/editar consultas, usa repositorios existentes (`IGymRepository`, `IUserRepository`, etc.) en lugar de acceder directo al `DbContext` desde Functions.

## Respuestas y manejo de errores
- Siempre devuelve `ApiResponse<T>` con `Success`, `Message`, `Data`, `StatusCode` coherente.
- 404 cuando el servicio indica no encontrado; 400 para validaciones/errores de solicitud; 500 sólo para fallos inesperados (logear con `ILogger`).

## Pagos y servicios externos
- Pasarelas: `PaymentsOptions.GatewayProvider` resuelve el servicio (`IPaymentGatewayService`) en `Program.cs`.
  - 1: PayU, 2: Wompi, 3: MercadoPago (por defecto), 4: Stripe (placeholder usa MercadoPago).
- Configurar URLs `Payments:{BaseSuccessUrl,BaseFailureUrl,BasePendingUrl}`. Hay `MercadoPagoGatewayRepository` con `HttpClient` registrado.
- Al agregar endpoints de pagos, usar `IPaymentIntentService`/`IPaymentService` y no llamar a la pasarela directo desde Functions.

## Almacenamiento y cache
- Blob Storage: inyecta `IBlobStorageService` (implementación `AzureBlobStorageService`, `BlobServiceClient` singleton) para subir/leer blobs.
- Redis: usa `IRedisCacheService` para cachear lecturas frecuentes (conexión configurada por `Redis:ConnectionString`).

## Convenciones de rutas y nombres
- Rutas HTTP legibles en snake-case de recurso: `user/{id}`, `users/find`, etc.
- Nombre de Function con prefijo de módulo: `User_GetUserByIdFunction`, `Plan_CreatePlanFunction`, etc.
- Carpetas por módulo en `FitGymApp/Functions/<Modulo>/*Function.cs`.

## Pruebas
- Framework: xUnit + Moq en `Gymmetry.Tests/**`.
- Referencia a proyectos: tests cubren Application/Domain/Repository y a veces Functions. Revisa `Gym/User/Payments` para ejemplos.
- Prioriza pruebas de servicios (Application) usando mocks de repositorios.

## Flujo de desarrollo
- Build/restore/test: `dotnet restore`, `dotnet build`, `dotnet test`.
- Ejecutar local (Functions): Azure Functions Core Tools (`func start`) leyendo `local.settings.json`.
- Variables locales: mantener en `FitGymApp/local.settings.json` bajo `Values:*` (no commitear credenciales). Claves comunes: `ConnectionStrings:DefaultConnection`, `Jwt:*`, `AzureStorage:ConnectionString`, `Payments:*`, `Redis:ConnectionString`.

## Antes de abrir PR
- Verifica que servicios/repo ya existen y reusa interfaces.
- Mantén `ApiResponse<T>` y validación JWT en todos los endpoints protegidos.
- Registra nuevas interfaces/servicios en `Program.cs` y agrega pruebas mínimas en `Gymmetry.Tests`.

¿Algo no quedó claro o falta un patrón clave (p. ej., un módulo específico de Functions)? Indícalo y ajusto estas instrucciones.
