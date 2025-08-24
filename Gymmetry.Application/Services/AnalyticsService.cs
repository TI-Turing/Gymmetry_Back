using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Analytics;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDailyRepository _dailyRepository;
        private readonly IRoutineAssignedRepository _routineAssignedRepository;
        private readonly IRoutineDayRepository _routineDayRepository;
        private readonly IPhysicalAssessmentRepository _physicalAssessmentRepository;
        private readonly ILogErrorService _logErrorService;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            IDailyRepository dailyRepository,
            IRoutineAssignedRepository routineAssignedRepository,
            IRoutineDayRepository routineDayRepository,
            IPhysicalAssessmentRepository physicalAssessmentRepository,
            ILogErrorService logErrorService,
            ILogger<AnalyticsService> logger)
        {
            _dailyRepository = dailyRepository;
            _routineAssignedRepository = routineAssignedRepository;
            _routineDayRepository = routineDayRepository;
            _physicalAssessmentRepository = physicalAssessmentRepository;
            _logErrorService = logErrorService;
            _logger = logger;
        }

        public async Task<ApplicationResponse<AnalyticsSummaryResponse>> GetSummaryAsync(AnalyticsSummaryRequest request)
        {
            try
            {
                _logger.LogInformation("[Analytics] Inicio Summary UserId={UserId} StartDate={StartDate} EndDate={EndDate} TZ={TZ}", request.UserId, request.StartDate, request.EndDate, request.Timezone ?? "(server default)");

                // Timezone
                var tzId = string.IsNullOrWhiteSpace(request.Timezone) ? TimeZoneInfo.Local.Id : request.Timezone;
                TimeZoneInfo tzInfo;
                try { tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
                catch { _logger.LogWarning("[Analytics] Timezone no válida: {TZ}, usando Local", tzId); tzInfo = TimeZoneInfo.Local; }

                // Parse dates (YYYY-MM-DD)
                if (!DateTime.TryParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startLocalDate))
                {
                    _logger.LogWarning("[Analytics] StartDate con formato inesperado: {StartDate}", request.StartDate);
                    startLocalDate = DateTime.SpecifyKind(DateTime.Parse(request.StartDate), DateTimeKind.Unspecified);
                }
                if (!DateTime.TryParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endLocalDate))
                {
                    _logger.LogWarning("[Analytics] EndDate con formato inesperado: {EndDate}", request.EndDate);
                    endLocalDate = DateTime.SpecifyKind(DateTime.Parse(request.EndDate), DateTimeKind.Unspecified);
                }

                var startLocal = DateTime.SpecifyKind(startLocalDate.Date, DateTimeKind.Unspecified);
                var endLocal = DateTime.SpecifyKind(endLocalDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
                _logger.LogInformation("[Analytics] Ventana UTC: {StartUtc:o}..{EndUtc:o}", startUtc, endUtc);

                // Data: dailies in range
                var dailies = await _dailyRepository.GetUserDailiesInRangeAsync(request.UserId, startUtc, endUtc);
                _logger.LogInformation("[Analytics] Dailies recuperados: {Count}", dailies.Count);

                var dailyLocal = dailies.Select(d => new
                {
                    Daily = d,
                    LocalDate = TimeZoneInfo.ConvertTimeFromUtc(d.CreatedAt, tzInfo).Date,
                    Advanced = d.Percentage > 0,
                    DurationMinutes = (d.EndDate > d.StartDate) ? (int)(d.EndDate - d.StartDate).TotalMinutes : 0,
                    Calories = 0m,
                    RoutineTemplateId = d.RoutineDay?.RoutineTemplateId,
                    RoutineTemplateName = d.RoutineDay?.RoutineTemplate?.Name,
                    Weekday = (int)(TimeZoneInfo.ConvertTimeFromUtc(d.CreatedAt, tzInfo).DayOfWeek == DayOfWeek.Sunday ? 7 : (int)TimeZoneInfo.ConvertTimeFromUtc(d.CreatedAt, tzInfo).DayOfWeek)
                }).ToList();

                var advancedByDate = dailyLocal
                    .GroupBy(x => x.LocalDate)
                    .ToDictionary(g => g.Key, g => g.Any(x => x.Advanced));

                var daysAdvanced = advancedByDate.Count(kv => kv.Value);
                var totalWorkouts = dailyLocal.Count(x => x.Advanced);
                var totalDuration = dailyLocal.Where(x => x.Advanced).Sum(x => x.DurationMinutes);
                var avgDuration = totalWorkouts > 0 ? (int)Math.Floor((double)totalDuration / totalWorkouts) : 0;
                _logger.LogInformation("[Analytics] Totales: Workouts={Workouts} Duration={Duration} Avg={Avg} DaysAdvanced={DaysAdv}", totalWorkouts, totalDuration, avgDuration, daysAdvanced);

                // Active routine(s)
                var activeAssignments = (await _routineAssignedRepository.GetActiveByUserAsync(request.UserId)).ToList();
                if (!activeAssignments.Any())
                {
                    var latest = await _routineAssignedRepository.GetLatestByUserUntilAsync(request.UserId, endUtc);
                    if (latest != null) activeAssignments.Add(latest);
                }
                var activeTemplateIds = activeAssignments.Select(a => a.RoutineTemplateId).Distinct().ToList();
                _logger.LogInformation("[Analytics] Templates activos (o último): {Templates}", string.Join(",", activeTemplateIds));

                var activeRoutineDays = new List<RoutineDay>();
                if (activeTemplateIds.Any())
                {
                    foreach (var tid in activeTemplateIds)
                    {
                        var part = await _routineDayRepository.FindRoutineDaysByFieldsAsync(new Dictionary<string, object>{{"RoutineTemplateId", tid},{"IsActive", true}});
                        activeRoutineDays.AddRange(part);
                    }
                }
                var expectedWeekdays = new HashSet<int>(activeRoutineDays.Select(rd => rd.DayNumber));

                // DaysExpected over range
                int daysExpected = 0;
                for (var dt = startLocalDate.Date; dt <= endLocalDate.Date; dt = dt.AddDays(1))
                {
                    var wd = (int)(dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek);
                    if (expectedWeekdays.Contains(wd)) daysExpected++;
                }
                _logger.LogInformation("[Analytics] DaysExpected={DaysExpected}", daysExpected);

                // Streaks
                int currentStreak = 0;
                for (var dt = endLocalDate.Date; dt >= startLocalDate.Date; dt = dt.AddDays(-1))
                {
                    if (advancedByDate.TryGetValue(dt, out var adv) && adv) currentStreak++;
                    else break;
                }
                int longestStreak = 0;
                int temp = 0;
                for (var dt = startLocalDate.Date; dt <= endLocalDate.Date; dt = dt.AddDays(1))
                {
                    if (advancedByDate.TryGetValue(dt, out var adv) && adv) { temp++; longestStreak = Math.Max(longestStreak, temp); }
                    else temp = 0;
                }
                _logger.LogInformation("[Analytics] Streaks: Current={Current} Longest={Longest}", currentStreak, longestStreak);

                // RoutineUsage
                var routineUsage = dailyLocal
                    .Where(x => x.Advanced && x.RoutineTemplateId.HasValue)
                    .GroupBy(x => new { x.RoutineTemplateId, x.RoutineTemplateName })
                    .Select(g => new RoutineUsageItem
                    {
                        RoutineTemplateId = g.Key.RoutineTemplateId!.Value,
                        RoutineTemplateName = g.Key.RoutineTemplateName ?? string.Empty,
                        DaysUsed = g.Select(x => x.LocalDate).Distinct().Count(),
                        DaysExpected = CountExpectedDaysForTemplate(activeRoutineDays, g.Key.RoutineTemplateId!.Value, startLocalDate.Date, endLocalDate.Date),
                        UsagePercent = 0m
                    })
                    .ToList();
                foreach (var ru in routineUsage)
                    ru.UsagePercent = Math.Round((decimal)ru.DaysUsed / Math.Max(ru.DaysExpected, 1) * 100m, 2);
                _logger.LogInformation("[Analytics] RoutineUsage items={Count}", routineUsage.Count);

                // WeekdayDiscipline
                var weekdayDiscipline = new List<WeekdayDisciplineItem>();
                for (int wd = 1; wd <= 7; wd++)
                {
                    var expectedDays = 0;
                    for (var dt = startLocalDate.Date; dt <= endLocalDate.Date; dt = dt.AddDays(1))
                        if ((int)(dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek) == wd && expectedWeekdays.Contains(wd)) expectedDays++;
                    var advancedDays = advancedByDate.Where(kv => kv.Key >= startLocalDate.Date && kv.Key <= endLocalDate.Date)
                        .Count(kv => kv.Value && ((int)(kv.Key.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)kv.Key.DayOfWeek) == wd));
                    weekdayDiscipline.Add(new WeekdayDisciplineItem { Weekday = wd, DaysAdvanced = advancedDays, DaysExpected = expectedDays });
                }

                // Branch attendance
                var advancedDailies = dailyLocal.Where(x => x.Advanced && x.Daily.BranchId.HasValue).ToList();
                var totalVisits = advancedDailies.Count;
                var branchAttendance = advancedDailies
                    .GroupBy(x => new { x.Daily.BranchId, x.Daily.Branch!.Name })
                    .Select(g => new BranchAttendanceItem
                    {
                        BranchId = g.Key.BranchId!.Value,
                        BranchName = g.Key.Name,
                        Visits = g.Count(),
                        Percent = totalVisits > 0 ? Math.Round(100m * g.Count() / totalVisits, 2) : 0m
                    })
                    .OrderByDescending(x => x.Visits)
                    .ToList();
                _logger.LogInformation("[Analytics] BranchAttendance items={Count}", branchAttendance.Count);

                // DailySeries per day
                var series = new List<DailySeriesItem>();
                for (var dt = startLocalDate.Date; dt <= endLocalDate.Date; dt = dt.AddDays(1))
                {
                    var items = dailyLocal.Where(x => x.LocalDate == dt).ToList();
                    var adv = items.Any(x => x.Advanced);
                    var dur = items.Where(x => x.Advanced).Sum(x => x.DurationMinutes);
                    var cal = 0m;
                    series.Add(new DailySeriesItem { Date = dt.ToString("yyyy-MM-dd"), Advanced = adv, DurationMinutes = dur, Calories = cal });
                }
                _logger.LogInformation("[Analytics] DailySeries puntos={Count}", series.Count);

                // Weight
                decimal? currentWeight = null;
                decimal? startWeight = null;
                try
                {
                    var physAll = await _physicalAssessmentRepository.GetAllPhysicalAssessmentsAsync();
                    var userPhys = physAll.Where(p => p.UserId == request.UserId).OrderByDescending(p => p.CreatedAt).ToList();
                    if (userPhys.Any())
                    {
                        currentWeight = ParseDecimal(userPhys.First().Weight);
                        var baseline = userPhys.Where(p => p.CreatedAt <= startUtc).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                        if (baseline != null) startWeight = ParseDecimal(baseline.Weight);
                    }
                }
                catch (Exception exW)
                {
                    _logger.LogWarning(exW, "[Analytics] No fue posible calcular peso desde PhysicalAssessment");
                }

                var response = new AnalyticsSummaryResponse
                {
                    TotalWorkouts = totalWorkouts,
                    TotalCalories = 0,
                    TotalDurationMinutes = totalDuration,
                    AvgDurationMinutes = avgDuration,
                    DaysAdvanced = daysAdvanced,
                    DaysExpected = daysExpected,
                    CurrentStreakDays = currentStreak,
                    LongestStreakDays = longestStreak,
                    CurrentWeightKg = currentWeight,
                    WeightChangeKg = (currentWeight.HasValue && startWeight.HasValue) ? currentWeight - startWeight : null,
                    RoutineUsage = routineUsage,
                    WeekdayDiscipline = weekdayDiscipline,
                    BranchAttendance = branchAttendance,
                    DailySeries = series,
                    GeneratedAt = DateTime.UtcNow.ToString("O")
                };

                _logger.LogInformation("[Analytics] Fin Summary UserId={UserId} Totals: Wkts={Workouts} DaysAdv={DaysAdv} DaysExp={DaysExp}", request.UserId, totalWorkouts, daysAdvanced, daysExpected);

                return new ApplicationResponse<AnalyticsSummaryResponse>
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Analytics] Error en Summary");
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<AnalyticsSummaryResponse>
                {
                    Success = false,
                    Message = "Error técnico al obtener el resumen de analytics."
                };
            }
        }

        private static int CountExpectedDaysForTemplate(List<RoutineDay> activeRoutineDays, Guid templateId, DateTime startDate, DateTime endDate)
        {
            var days = new HashSet<int>(activeRoutineDays.Where(rd => rd.RoutineTemplateId == templateId).Select(rd => rd.DayNumber));
            if (!days.Any()) return 0;
            int count = 0;
            for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
            {
                var wd = (int)(dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek);
                if (days.Contains(wd)) count++;
            }
            return count;
        }

        private static decimal? ParseDecimal(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return null;
        }
    }
}
