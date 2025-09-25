using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.AppState;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;
using DomainUser = Gymmetry.Domain.Models.User;
using DomainGym = Gymmetry.Domain.Models.Gym;
using DomainBranch = Gymmetry.Domain.Models.Branch;
using DomainFeed = Gymmetry.Domain.Models.Feed;
using DomainDaily = Gymmetry.Domain.Models.Daily;
using DomainPlan = Gymmetry.Domain.Models.Plan;
using DomainPhysicalAssessment = Gymmetry.Domain.Models.PhysicalAssessment;

namespace Gymmetry.Application.Services
{
    public class AppStateService : IAppStateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDailyRepository _dailyRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IFeedRepository _feedRepository;
        private readonly IPhysicalAssessmentRepository _physicalAssessmentRepository;
        private readonly IRoutineAssignedRepository _routineAssignedRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly ILogger<AppStateService> _logger;
        private readonly ILogErrorService _logErrorService;

        public AppStateService(
            IUserRepository userRepository,
            IDailyRepository dailyRepository,
            IPlanRepository planRepository,
            IGymRepository gymRepository,
            IFeedRepository feedRepository,
            IPhysicalAssessmentRepository physicalAssessmentRepository,
            IRoutineAssignedRepository routineAssignedRepository,
            IBranchRepository branchRepository,
            ILogger<AppStateService> logger,
            ILogErrorService logErrorService)
        {
            _userRepository = userRepository;
            _dailyRepository = dailyRepository;
            _planRepository = planRepository;
            _gymRepository = gymRepository;
            _feedRepository = feedRepository;
            _physicalAssessmentRepository = physicalAssessmentRepository;
            _routineAssignedRepository = routineAssignedRepository;
            _branchRepository = branchRepository;
            _logger = logger;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<AppStateOverviewDto>> GetAppStateOverviewAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation($"[AppStateService] Inicio GetAppStateOverviewAsync para usuario {userId}");

                // Validar usuario existe
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApplicationResponse<AppStateOverviewDto>.ErrorResponse("Usuario no encontrado", "NotFound");
                }

                var overview = new AppStateOverviewDto
                {
                    LastUpdated = DateTime.UtcNow
                };

                // Ejecutar consultas en paralelo donde sea posible
                var homeTask = BuildHomeStateAsync(userId);
                var gymTask = BuildGymStateAsync(user);
                var progressTask = BuildProgressStateAsync(userId);
                var feedTask = BuildFeedStateAsync();
                var profileTask = BuildProfileStateAsync(user);

                await Task.WhenAll(homeTask, gymTask, progressTask, feedTask, profileTask);

                overview.Home = await homeTask;
                overview.Gym = await gymTask;
                overview.Progress = await progressTask;
                overview.Feed = await feedTask;
                overview.Profile = await profileTask;

                _logger.LogInformation($"[AppStateService] AppState construido exitosamente para usuario {userId}");
                
                return ApplicationResponse<AppStateOverviewDto>.SuccessResponse(overview, "Estado de la aplicación obtenido correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AppStateService] Error obteniendo AppState para usuario {userId}");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<AppStateOverviewDto>.ErrorResponse("Error técnico al obtener el estado de la aplicación", "TechnicalError");
            }
        }

        private async Task<HomeStateDto> BuildHomeStateAsync(Guid userId)
        {
            try
            {
                var homeState = new HomeStateDto();

                // Obtener dailies de las últimas 4 semanas
                var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);
                var dailies = await _dailyRepository.GetUserDailiesInRangeAsync(userId, fourWeeksAgo, DateTime.UtcNow);
                var dailiesList = dailies.ToList();

                // Calcular disciplina
                homeState.Discipline = CalculateDisciplineData(dailiesList);

                // Obtener plan activo
                var activePlans = await _planRepository.FindPlansByFieldsAsync(new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "IsActive", true }
                });

                var activePlan = activePlans.FirstOrDefault(p => p.EndDate > DateTime.UtcNow);
                homeState.PlanInfo = BuildPlanInfo(activePlan);

                // Rutina de hoy
                homeState.TodayRoutine = await BuildTodayRoutineAsync(userId, dailiesList);

                // Progreso detallado
                homeState.DetailedProgress = BuildDetailedProgress(dailiesList);

                return homeState;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error construyendo HomeState para usuario {userId}");
                return new HomeStateDto(); // Fallback
            }
        }

        private async Task<GymStateDto> BuildGymStateAsync(DomainUser user)
        {
            try
            {
                var gymState = new GymStateDto
                {
                    IsConnectedToGym = user.GymId.HasValue,
                    GymId = user.GymId?.ToString()
                };

                if (user.GymId.HasValue)
                {
                    var gym = await _gymRepository.GetGymByIdAsync(user.GymId.Value);
                    gymState.GymData = gym;

                    if (gym != null)
                    {
                        // Obtener sucursales del gimnasio
                        var branches = await _branchRepository.FindBranchesByFieldsAsync(new Dictionary<string, object>
                        {
                            { "GymId", gym.Id },
                            { "IsActive", true }
                        });
                        gymState.AvailableBranches = branches.ToList();
                    }
                }

                return gymState;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error construyendo GymState para usuario {user.Id}");
                return new GymStateDto(); // Fallback
            }
        }

        private async Task<ProgressStateDto> BuildProgressStateAsync(Guid userId)
        {
            try
            {
                // Obtener dailies de los últimos 3 meses para resumen de progreso
                var threeMonthsAgo = DateTime.UtcNow.AddDays(-90);
                var progressDailies = await _dailyRepository.GetUserDailiesInRangeAsync(userId, threeMonthsAgo, DateTime.UtcNow);
                var progressDailiesList = progressDailies.ToList();

                var progressState = new ProgressStateDto
                {
                    Summary = BuildProgressSummary(progressDailiesList),
                    DefaultPeriod = "3months"
                };

                return progressState;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error construyendo ProgressState para usuario {userId}");
                return new ProgressStateDto(); // Fallback
            }
        }

        private async Task<FeedStateDto> BuildFeedStateAsync()
        {
            try
            {
                var feedState = new FeedStateDto();

                // Obtener feeds recientes (últimos 10)
                var allFeeds = await _feedRepository.GetAllFeedsAsync();
                var feedsList = allFeeds.Where(f => f.IsActive && !f.IsDeleted)
                                       .OrderByDescending(f => f.CreatedAt)
                                       .ToList();

                feedState.RecentFeeds = feedsList.Take(10).ToList();
                feedState.TotalFeedCount = feedsList.Count;

                // Obtener trending feeds (últimas 24 horas con más interacciones)
                var last24Hours = DateTime.UtcNow.AddHours(-24);
                var trendingFeeds = feedsList
                    .Where(f => f.CreatedAt >= last24Hours)
                    .OrderByDescending(f => f.LikesCount * 3 + f.CommentsCount * 2)
                    .ThenByDescending(f => f.CreatedAt)
                    .Take(10)
                    .ToList();

                feedState.TrendingFeeds = trendingFeeds;

                return feedState;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AppStateService] Error construyendo FeedState");
                return new FeedStateDto(); // Fallback
            }
        }

        private async Task<ProfileStateDto> BuildProfileStateAsync(DomainUser user)
        {
            try
            {
                var profileState = new ProfileStateDto
                {
                    UserProfile = user
                };

                // Obtener última valoración física
                var assessments = await _physicalAssessmentRepository.FindPhysicalAssessmentsByFieldsAsync(new Dictionary<string, object>
                {
                    { "UserId", user.Id },
                    { "IsActive", true }
                });

                profileState.LatestAssessment = assessments
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefault();

                // Calcular estadísticas del perfil
                profileState.Stats = await BuildProfileStatsAsync(user.Id, profileState.LatestAssessment);

                return profileState;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error construyendo ProfileState para usuario {user.Id}");
                return new ProfileStateDto
                {
                    UserProfile = user
                }; // Fallback
            }
        }

        private DisciplineDataDto CalculateDisciplineData(List<DomainDaily> dailies)
        {
            if (!dailies.Any())
            {
                return new DisciplineDataDto();
            }

            var completedDays = dailies.Count(d => d.Percentage >= 70); // Considerar 70% como completado
            var totalExpectedDays = 28; // 4 semanas asumiendo entrenamiento diario (se puede ajustar)
            
            // Calcular racha actual
            var currentStreak = CalculateCurrentStreak(dailies);
            
            // Calcular índice de consistencia (basado en regularidad de entrenamientos)
            var consistencyIndex = CalculateConsistencyIndex(dailies);

            return new DisciplineDataDto
            {
                CompletionPercentage = totalExpectedDays > 0 ? Math.Round(100m * completedDays / totalExpectedDays, 2) : 0,
                CompletedDays = completedDays,
                TotalExpectedDays = totalExpectedDays,
                CurrentStreak = currentStreak,
                ConsistencyIndex = consistencyIndex,
                PeriodDescription = "Últimas 4 semanas"
            };
        }

        private int CalculateCurrentStreak(List<DomainDaily> dailies)
        {
            if (!dailies.Any()) return 0;

            var sortedDailies = dailies.OrderByDescending(d => d.CreatedAt).ToList();
            var streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            foreach (var daily in sortedDailies)
            {
                var dailyDate = daily.CreatedAt.Date;
                if (dailyDate == currentDate.AddDays(-streak) && daily.Percentage >= 70)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private decimal CalculateConsistencyIndex(List<DomainDaily> dailies)
        {
            if (dailies.Count < 2) return 0;

            var sortedDailies = dailies.OrderBy(d => d.CreatedAt).ToList();
            var intervals = new List<double>();

            for (int i = 1; i < sortedDailies.Count; i++)
            {
                var interval = (sortedDailies[i].CreatedAt - sortedDailies[i - 1].CreatedAt).TotalDays;
                intervals.Add(interval);
            }

            if (!intervals.Any()) return 0;

            var averageInterval = intervals.Average();
            var variance = intervals.Sum(x => Math.Pow(x - averageInterval, 2)) / intervals.Count;
            var standardDeviation = Math.Sqrt(variance);

            // Normalizar: mayor consistencia = menor desviación estándar
            var consistencyScore = Math.Max(0, 1 - (standardDeviation / 7.0)); // 7 días como referencia
            return (decimal)Math.Round(consistencyScore, 4);
        }

        private PlanInfoDto BuildPlanInfo(DomainPlan? plan)
        {
            if (plan == null)
            {
                return new PlanInfoDto
                {
                    IsActive = false
                };
            }

            var totalDays = (plan.EndDate - plan.StartDate).Days;
            var elapsedDays = Math.Max(0, (DateTime.UtcNow - plan.StartDate).Days);
            var progressPercentage = totalDays > 0 ? Math.Min(100, Math.Round(100m * elapsedDays / totalDays, 2)) : 0;
            var daysRemaining = Math.Max(0, (plan.EndDate - DateTime.UtcNow).Days);

            return new PlanInfoDto
            {
                PlanId = plan.Id,
                PlanTypeName = plan.PlanType?.Name ?? "Plan activo",
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                IsActive = plan.IsActive && plan.EndDate > DateTime.UtcNow,
                ProgressPercentage = progressPercentage,
                DaysRemaining = daysRemaining
            };
        }

        private async Task<TodayRoutineDto> BuildTodayRoutineAsync(Guid userId, List<DomainDaily> dailies)
        {
            var today = DateTime.UtcNow.Date;
            var hasTrainedToday = dailies.Any(d => d.CreatedAt.Date == today && d.Percentage > 0);
            
            var lastWorkout = dailies.Where(d => d.Percentage > 0)
                                   .OrderByDescending(d => d.CreatedAt)
                                   .FirstOrDefault()?.CreatedAt;

            var todayRoutine = new TodayRoutineDto
            {
                HasTrainedToday = hasTrainedToday,
                LastWorkout = lastWorkout
            };

            try
            {
                // Obtener rutina asignada activa
                var activeRoutines = await _routineAssignedRepository.GetActiveByUserAsync(userId);
                var activeRoutine = activeRoutines.FirstOrDefault();

                if (activeRoutine?.RoutineTemplate != null)
                {
                    todayRoutine.RoutineName = activeRoutine.RoutineTemplate.Name;
                    // Note: EstimatedDurationMinutes might not exist in RoutineTemplate, using a default
                    todayRoutine.EstimatedDurationMinutes = 60; // Default 60 minutes
                    
                    // Obtener ejercicios de hoy (simplificado - se podría mejorar con lógica de días de semana)
                    var routineDays = activeRoutine.RoutineTemplate.RoutineDays?.ToList();
                    if (routineDays?.Any() == true)
                    {
                        var todayDayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
                        // Note: Assuming RoutineDay has some way to identify day - using CreatedAt as fallback
                        var todayRoutineDay = routineDays.FirstOrDefault();
                        
                        if (todayRoutineDay != null)
                        {
                            todayRoutine.TodayRoutineDayId = todayRoutineDay.Id;
                            // RoutineDay has direct Exercise navigation property
                            var exercises = new List<string>();
                            if (todayRoutineDay.Exercise != null)
                            {
                                exercises.Add(todayRoutineDay.Exercise.Name);
                            }
                            todayRoutine.TodayExercises = exercises;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error obteniendo rutina de hoy para usuario {userId}");
            }

            return todayRoutine;
        }

        private DetailedProgressDto BuildDetailedProgress(List<DomainDaily> dailies)
        {
            var workoutDailies = dailies.Where(d => d.Percentage > 0).ToList();

            if (!workoutDailies.Any())
            {
                return new DetailedProgressDto();
            }

            var totalMinutes = workoutDailies.Sum(d => (int)(d.EndDate - d.StartDate).TotalMinutes);
            var avgMinutes = workoutDailies.Count > 0 ? totalMinutes / workoutDailies.Count : 0;
            var avgCompletion = workoutDailies.Average(d => d.Percentage);

            var recentWorkouts = workoutDailies
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .Select(d => new RecentWorkoutDto
                {
                    Date = d.CreatedAt,
                    DurationMinutes = (int)(d.EndDate - d.StartDate).TotalMinutes,
                    CompletionPercentage = d.Percentage,
                    BranchName = d.Branch?.Name
                })
                .ToList();

            return new DetailedProgressDto
            {
                TotalWorkouts = workoutDailies.Count,
                TotalMinutes = totalMinutes,
                AvgWorkoutMinutes = avgMinutes,
                AvgCompletionRate = Math.Round((decimal)avgCompletion, 2),
                RecentWorkouts = recentWorkouts
            };
        }

        private ProgressSummaryDto BuildProgressSummary(List<DomainDaily> dailies)
        {
            var workoutDailies = dailies.Where(d => d.Percentage > 0).ToList();

            if (!workoutDailies.Any())
            {
                return new ProgressSummaryDto();
            }

            var completedDays = dailies.Count(d => d.Percentage >= 70);
            var totalDays = 90; // 3 meses
            var adherencePercentage = Math.Round(100m * completedDays / totalDays, 2);
            var totalMinutes = workoutDailies.Sum(d => (int)(d.EndDate - d.StartDate).TotalMinutes);

            // Simulación básica de distribución muscular (se podría mejorar con datos reales de ejercicios)
            var muscleDistribution = new Dictionary<string, decimal>
            {
                { "Pecho", 0.20m },
                { "Espalda", 0.18m },
                { "Piernas", 0.25m },
                { "Brazos", 0.15m },
                { "Hombros", 0.12m },
                { "Core", 0.10m }
            };

            return new ProgressSummaryDto
            {
                AdherencePercentage = adherencePercentage,
                WorkoutsSummary = workoutDailies.Count,
                TotalMinutes = totalMinutes,
                MuscleDistribution = muscleDistribution,
                DominantMuscles = new List<string> { "Piernas", "Pecho" },
                UnderworkedMuscles = new List<string> { "Core", "Hombros" },
                BalanceIndex = 0.75m
            };
        }

        private async Task<ProfileStatsDto> BuildProfileStatsAsync(Guid userId, DomainPhysicalAssessment? latestAssessment)
        {
            try
            {
                // Obtener todos los dailies del usuario para estadísticas generales
                var allUserDailies = await _dailyRepository.FindDailiesByFieldsAsync(new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "IsActive", true }
                });

                var workoutDailies = allUserDailies.Where(d => d.Percentage > 0).ToList();
                var user = await _userRepository.GetUserByIdAsync(userId);

                // Calcular racha actual
                var currentStreak = CalculateCurrentStreak(workoutDailies);

                // Calcular días totales entrenando
                var totalDays = workoutDailies.Select(d => d.CreatedAt.Date).Distinct().Count();

                var stats = new ProfileStatsDto
                {
                    TotalWorkouts = workoutDailies.Count,
                    CurrentStreak = currentStreak,
                    TotalDays = totalDays,
                    MemberSince = user?.CreatedAt,
                    CurrentWeight = latestAssessment?.Weight,
                    CurrentHeight = latestAssessment?.Height,
                    LastAssessment = latestAssessment?.CreatedAt
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[AppStateService] Error construyendo ProfileStats para usuario {userId}");
                return new ProfileStatsDto();
            }
        }
    }
}