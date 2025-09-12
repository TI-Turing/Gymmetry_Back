using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Progress;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class ProgressReportService : IProgressReportService
    {
        private readonly IDailyRepository _dailyRepository;
        private readonly IDailyHistoryRepository _dailyHistoryRepository;
        private readonly IDailyExerciseRepository _dailyExerciseRepository;
        private readonly IDailyExerciseHistoryRepository _dailyExerciseHistoryRepository;
        private readonly IRoutineDayRepository _routineDayRepository;
        private readonly IRoutineAssignedRepository _routineAssignedRepository;
        private readonly IPhysicalAssessmentRepository _physicalAssessmentRepository;
        private readonly ILogErrorService _logErrorService;
        private readonly ILogger<ProgressReportService> _logger;

        public ProgressReportService(
            IDailyRepository dailyRepository,
            IDailyHistoryRepository dailyHistoryRepository,
            IDailyExerciseRepository dailyExerciseRepository,
            IDailyExerciseHistoryRepository dailyExerciseHistoryRepository,
            IRoutineDayRepository routineDayRepository,
            IRoutineAssignedRepository routineAssignedRepository,
            IPhysicalAssessmentRepository physicalAssessmentRepository,
            ILogErrorService logErrorService,
            ILogger<ProgressReportService> logger)
        {
            _dailyRepository = dailyRepository;
            _dailyHistoryRepository = dailyHistoryRepository;
            _dailyExerciseRepository = dailyExerciseRepository;
            _dailyExerciseHistoryRepository = dailyExerciseHistoryRepository;
            _routineDayRepository = routineDayRepository;
            _routineAssignedRepository = routineAssignedRepository;
            _physicalAssessmentRepository = physicalAssessmentRepository;
            _logErrorService = logErrorService;
            _logger = logger;
        }

        public async Task<ApplicationResponse<ProgressSummaryResponse>> GetSummaryAsync(ProgressReportRequest request)
        {
            try
            {
                var tzId = string.IsNullOrWhiteSpace(request.Timezone) ? TimeZoneInfo.Local.Id : request.Timezone;
                TimeZoneInfo tzInfo;
                try { tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId); } catch { tzInfo = TimeZoneInfo.Local; }

                if (!DateTime.TryParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateLocal))
                    startDateLocal = DateTime.Parse(request.StartDate);
                if (!DateTime.TryParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateLocal))
                    endDateLocal = DateTime.Parse(request.EndDate);

                var startLocal = startDateLocal.Date;
                var endLocal = endDateLocal.Date;
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), tzInfo);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified), tzInfo);

                // Dailies (recent window)
                var dailies = await _dailyRepository.GetUserDailiesInRangeAsync(request.UserId!.Value, startUtc, endUtc);
                // Históricos (older than period start?) - opcional ahora (unificado más adelante). No se mezclan aún.

                var summary = new ProgressSummaryResponse
                {
                    Period = new PeriodInfo { From = startLocal.ToString("yyyy-MM-dd"), To = endLocal.ToString("yyyy-MM-dd"), Days = (endLocal - startLocal).Days + 1 }
                };

                BuildAdherenceAndDiscipline(summary, dailies, startLocal, endLocal, request.MinCompletionForAdherence, tzInfo);
                BuildExecutionAndTime(summary, dailies, tzInfo);
                await BuildExercisesObjectivesAndMusclesAsync(summary, dailies, request.TopExercises);
                if (request.ComparePreviousPeriod) BuildComparison(summary, dailies, startLocal, endLocal);
                if (request.IncludeAssessments) await BuildAssessmentsAsync(summary, request.UserId!.Value, startUtc);
                BuildSuggestions(summary);

                return new ApplicationResponse<ProgressSummaryResponse> { Success = true, Data = summary };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                _logger.LogError(ex, "[ProgressReport] Error generando reporte");
                return new ApplicationResponse<ProgressSummaryResponse> { Success = false, Message = "Error técnico al generar el reporte de progreso." };
            }
        }

        private void BuildAdherenceAndDiscipline(ProgressSummaryResponse summary, IReadOnlyList<Daily> dailies, DateTime startLocal, DateTime endLocal, int minCompletion, TimeZoneInfo tz)
        {
            var byDate = dailies
                .GroupBy(d => TimeZoneInfo.ConvertTimeFromUtc(d.CreatedAt, tz).Date)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Percentage).First());
            int completed = byDate.Count(kv => kv.Value.Percentage >= minCompletion);
            int sessions = dailies.Count(d => d.Percentage > 0);
            // Expected days from active routine days (approx): distinct weekday values encountered
            var weekdays = new HashSet<int>(dailies.Select(d => (int)TimeZoneInfo.ConvertTimeFromUtc(d.CreatedAt, tz).DayOfWeek));
            int daysExpected = 0;
            for (var dt = startLocal; dt <= endLocal; dt = dt.AddDays(1)) if (weekdays.Contains((int)dt.DayOfWeek)) daysExpected++;

            // Streaks
            int current = 0; int longest = 0; int temp = 0;
            for (var dt = startLocal; dt <= endLocal; dt = dt.AddDays(1))
            {
                bool adv = byDate.TryGetValue(dt, out var d) && d.Percentage >= minCompletion;
                if (adv) { temp++; longest = Math.Max(longest, temp); }
                else temp = 0;
            }
            // current streak backwards
            for (var dt = endLocal; dt >= startLocal; dt = dt.AddDays(-1))
            {
                bool adv = byDate.TryGetValue(dt, out var d) && d.Percentage >= minCompletion;
                if (adv) current++; else break;
            }

            summary.Adherence.TargetDays = daysExpected;
            summary.Adherence.Sessions = sessions;
            summary.Adherence.CompletedDays = completed;
            summary.Adherence.AdherencePct = daysExpected > 0 ? Math.Round(100m * completed / daysExpected, 2) : 0m;
            summary.Adherence.CurrentStreak = current;
            summary.Adherence.MaxStreak = longest;

            // Weekday breakdown
            for (int wd = 0; wd < 7; wd++)
            {
                var expected = 0; for (var dt = startLocal; dt <= endLocal; dt = dt.AddDays(1)) if ((int)dt.DayOfWeek == wd && weekdays.Contains(wd)) expected++;
                var done = byDate.Count(kv => kv.Key >= startLocal && kv.Key <= endLocal && (int)kv.Key.DayOfWeek == wd && kv.Value.Percentage >= minCompletion);
                summary.Adherence.ByWeekday.Add(new WeekdayBreakdown { Weekday = wd, Expected = expected, Done = done });
            }

            // Branch attendance
            var branchGroups = dailies.Where(d => d.BranchId.HasValue && d.Percentage > 0)
                .GroupBy(d => new { d.BranchId, d.Branch!.Name })
                .ToList();
            var totalVisits = branchGroups.Sum(g => g.Count());
            foreach (var g in branchGroups)
            {
                summary.Adherence.BranchAttendance.Add(new BranchUsage
                {
                    BranchId = g.Key.BranchId!.Value,
                    Name = g.Key.Name,
                    Visits = g.Count(),
                    Percent = totalVisits > 0 ? Math.Round(100m * g.Count() / totalVisits, 2) : 0m
                });
            }

            // Discipline (simple heuristics)
            var ordered = dailies.Where(d => d.Percentage > 0).OrderBy(d => d.StartDate).ToList();
            var deltas = new List<double>();
            for (int i = 1; i < ordered.Count; i++) deltas.Add((ordered[i].StartDate - ordered[i - 1].StartDate).TotalDays);
            double stdev = deltas.Count > 1 ? Math.Sqrt(deltas.Sum(x => Math.Pow(x - deltas.Average(), 2)) / (deltas.Count - 1)) : 0;
            summary.Discipline.ConsistencyIndex = deltas.Count > 0 ? (decimal)Math.Max(0, 1 - (stdev / 7.0)) : 0;
            var hours = ordered.Select(o => TimeZoneInfo.ConvertTimeFromUtc(o.StartDate, tz).Hour).ToList();
            if (hours.Any()) summary.Discipline.CommonStartHour = hours.GroupBy(h => h).OrderByDescending(g => g.Count()).First().Key.ToString("00") + ":00";
            double avgHour = hours.Count > 0 ? hours.Average(h => (double)h) : 0.0;
            double avgAbsDev = hours.Count > 0 ? hours.Select(h => Math.Abs(h - avgHour)).Average() : 0.0;
            double regularity = hours.Count > 1 ? Math.Max(0, 1 - (avgAbsDev / 12.0)) : 0.0;
            summary.Discipline.ScheduleRegularity = (decimal)Math.Round(regularity, 4);
        }

        private void BuildExecutionAndTime(ProgressSummaryResponse summary, IReadOnlyList<Daily> dailies, TimeZoneInfo tz)
        {
            var sessions = dailies.Where(d => d.Percentage > 0).ToList();
            if (!sessions.Any()) return;
            var percentages = sessions.Select(s => (decimal)s.Percentage).ToList();
            var avg = percentages.Average();
            var stdev = percentages.Count > 1 ? Math.Sqrt(percentages.Sum(p => Math.Pow((double)(p - (decimal)avg), 2)) / (percentages.Count - 1)) : 0;
            summary.Execution.AvgCompletion = Math.Round(avg, 2);
            summary.Execution.StdevCompletion = (decimal)Math.Round(stdev, 2);
            var best = sessions.OrderByDescending(s => s.Percentage).ThenByDescending(s => s.EndDate - s.StartDate).Take(5);
            var low = sessions.Where(s => s.Percentage < 30).OrderBy(s => s.Percentage).Take(5);
            foreach (var b in best)
                summary.Execution.BestSessions.Add(new SessionInfo { DailyId = b.Id, Date = TimeZoneInfo.ConvertTimeFromUtc(b.CreatedAt, tz).ToString("yyyy-MM-dd"), Percentage = b.Percentage, DurationMinutes = (int)(b.EndDate - b.StartDate).TotalMinutes });
            foreach (var l in low)
                summary.Execution.LowCompletionSessions.Add(new SessionInfo { DailyId = l.Id, Date = TimeZoneInfo.ConvertTimeFromUtc(l.CreatedAt, tz).ToString("yyyy-MM-dd"), Percentage = l.Percentage, DurationMinutes = (int)(l.EndDate - l.StartDate).TotalMinutes });
            summary.Time.TotalMinutes = sessions.Sum(s => (int)(s.EndDate - s.StartDate).TotalMinutes);
            summary.Time.AvgPerSession = sessions.Count > 0 ? (int)Math.Round(summary.Time.TotalMinutes / (double)sessions.Count) : 0;
            summary.Time.MinSession = sessions.Min(s => (int)(s.EndDate - s.StartDate).TotalMinutes);
            summary.Time.MaxSession = sessions.Max(s => (int)(s.EndDate - s.StartDate).TotalMinutes);
            // Daily series
            var byDate = sessions.GroupBy(s => TimeZoneInfo.ConvertTimeFromUtc(s.CreatedAt, tz).Date);
            foreach (var g in byDate)
            {
                var dateStr = g.Key.ToString("yyyy-MM-dd");
                summary.Execution.Series.Add(new DailyPoint { Date = dateStr, DurationMinutes = g.Sum(x => (int)(x.EndDate - x.StartDate).TotalMinutes), Percentage = (int)Math.Round(g.Average(x => x.Percentage)) });
            }
        }

        private async Task BuildExercisesObjectivesAndMusclesAsync(ProgressSummaryResponse summary, IReadOnlyList<Daily> dailies, int topN)
        {
            if (!dailies.Any()) return;
            // Obtener ejercicios ejecutados
            var dailyIds = dailies.Select(d => d.Id).Distinct().ToList();
            // Acceso directo a contexto no expuesto; usamos reflexión sobre repositorio DailyExerciseRepository si disponible via service provider sería mejor.
            // Simplificación: crear un scope EF mediante repositorio interno no disponible aquí -> asumimos navegación no cargada.
            // Fallback: intentar cargar con un repositorio nuevo no inyectado (no permitido). Entonces: simplificar -> query incremental.
            try
            {
                // Hack: usar DailyExerciseHistoryRepository no ayuda; necesitamos DailyExercise. Inyectar IDailyExerciseRepository (ya agregamos en ctor) y castear.
                var repo = _dailyExerciseRepository as Gymmetry.Repository.Services.DailyExerciseRepository;
                if (repo == null)
                {
                    _logger.LogWarning("[ProgressReport] IDailyExerciseRepository no es implementación esperada, no se puede cargar ejercicios.");
                    return;
                }
            }
            catch { }
            // Mejor: añadir método GetByDailyIdsAsync al interface y usarlo (ya añadido). Llamada segura con dynamic cast.
            IReadOnlyList<DailyExercise> executed = new List<DailyExercise>();
            try
            {
                var method = _dailyExerciseRepository.GetType().GetMethod("GetByDailyIdsAsync");
                if (method != null)
                {
                    var task = (Task)method.Invoke(_dailyExerciseRepository, new object[] { dailyIds })!;
                    await task.ConfigureAwait(false);
                    var resultProp = task.GetType().GetProperty("Result");
                    executed = (IReadOnlyList<DailyExercise>?)resultProp?.GetValue(task) ?? new List<DailyExercise>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProgressReport] No se pudieron cargar ejercicios ejecutados");
            }
            if (!executed.Any()) return;

            // Frecuencias
            var exerciseGroups = executed.GroupBy(e => e.ExerciseId).ToList();
            summary.Exercises.DistinctExercises = exerciseGroups.Count;
            foreach (var g in exerciseGroups)
            {
                var series = g.Count();
                int repsTotal = 0;
                foreach (var de in g)
                {
                    if (int.TryParse(de.Repetitions, out var r)) repsTotal += r;
                    else if (de.Repetitions?.Contains(',') == true)
                    {
                        repsTotal += de.Repetitions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s.Trim(), out var v) ? v : 0).Sum();
                    }
                }
                summary.Exercises.TotalSeries += series;
                summary.Exercises.TotalReps += repsTotal;
            }
            var totalSessions = dailies.Count(d => d.Percentage > 0);
            var freqList = exerciseGroups.Select(g => new ExerciseFreq
            {
                ExerciseId = g.Key,
                Name = g.First().Exercise.Name,
                Sessions = g.Select(x => x.DailyId).Distinct().Count(),
                Series = g.Count(),
                Reps = g.Sum(x =>
                {
                    if (int.TryParse(x.Repetitions, out var r)) return r;
                    if (x.Repetitions?.Contains(',') == true) return x.Repetitions.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s.Trim(), out var v) ? v : 0).Sum();
                    return 0;
                }),
                PercentSessions = totalSessions > 0 ? Math.Round(100m * g.Select(x => x.DailyId).Distinct().Count() / totalSessions, 2) : 0m
            }).OrderByDescending(f => f.Sessions).ThenByDescending(f => f.Series).ToList();
            summary.Exercises.TopExercises = freqList.Take(topN).ToList();
            summary.Exercises.UnderusedExercises = freqList.Where(f => f.PercentSessions < 10).Take(10).ToList();

            // Objetivos planificados (promedio de plantillas activas en las sesiones)
            var templateObjectiveAccum = new Dictionary<string, List<decimal>>();
            foreach (var d in dailies)
            {
                var json = d.RoutineDay?.RoutineTemplate?.TagsObjectives;
                if (string.IsNullOrWhiteSpace(json)) continue;
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json) ?? new();
                    foreach (var kv in dict)
                    {
                        if (!templateObjectiveAccum.ContainsKey(kv.Key)) templateObjectiveAccum[kv.Key] = new List<decimal>();
                        templateObjectiveAccum[kv.Key].Add(kv.Value);
                    }
                }
                catch { }
            }
            foreach (var kv in templateObjectiveAccum)
                summary.Objectives.Planned[kv.Key] = Math.Round(kv.Value.Average(), 4);

            // Objetivos ejecutados (ponderar por series del ejercicio)
            var executedObjectives = new Dictionary<string, decimal>();
            foreach (var ex in executed)
            {
                if (string.IsNullOrWhiteSpace(ex.Exercise.TagsObjectives)) continue;
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(ex.Exercise.TagsObjectives) ?? new();
                    var weight = 1m; // se podría usar series/reps
                    foreach (var kv in dict)
                    {
                        if (!executedObjectives.ContainsKey(kv.Key)) executedObjectives[kv.Key] = 0m;
                        executedObjectives[kv.Key] += (decimal)kv.Value * weight;
                    }
                }
                catch { }
            }
            // Normalizar ejecutados
            var totalObj = executedObjectives.Values.Sum();
            if (totalObj > 0)
            {
                foreach (var key in executedObjectives.Keys.ToList())
                    summary.Objectives.Executed[key] = Math.Round(executedObjectives[key] / totalObj, 4);
            }
            // Gaps
            foreach (var key in summary.Objectives.Planned.Keys.Union(summary.Objectives.Executed.Keys))
            {
                summary.Objectives.Gaps.Add(new ObjectiveGap
                {
                    Objective = key,
                    Planned = summary.Objectives.Planned.TryGetValue(key, out var pv) ? pv : 0,
                    Executed = summary.Objectives.Executed.TryGetValue(key, out var ev) ? ev : 0,
                    Gap = (summary.Objectives.Executed.TryGetValue(key, out var ev2) ? ev2 : 0) - (summary.Objectives.Planned.TryGetValue(key, out var pv2) ? pv2 : 0)
                });
            }

            // Músculos
            var muscleAccum = new Dictionary<string, decimal>();
            foreach (var ex in executed)
            {
                if (string.IsNullOrWhiteSpace(ex.Exercise.TagsMuscle)) continue;
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(ex.Exercise.TagsMuscle) ?? new();
                    foreach (var kv in dict)
                    {
                        if (!muscleAccum.ContainsKey(kv.Key)) muscleAccum[kv.Key] = 0m;
                        muscleAccum[kv.Key] += (decimal)kv.Value; // peso simple
                    }
                }
                catch { }
            }
            var totalMuscle = muscleAccum.Values.Sum();
            if (totalMuscle > 0)
            {
                foreach (var k in muscleAccum.Keys)
                    summary.Muscles.Distribution[k] = Math.Round(muscleAccum[k] / totalMuscle, 4);
            }
            // Dominantes / sub-atendidos
            if (summary.Muscles.Distribution.Any())
            {
                var avg = summary.Muscles.Distribution.Values.Average();
                summary.Muscles.Dominant = summary.Muscles.Distribution.Where(kv => kv.Value > avg * 1.4m).Select(kv => kv.Key).Take(5).ToList();
                summary.Muscles.Underworked = summary.Muscles.Distribution.Where(kv => kv.Value < avg * 0.5m).Select(kv => kv.Key).Take(5).ToList();
                // Balance index
                var variance = summary.Muscles.Distribution.Values.Select(v => Math.Pow((double)(v - (decimal)avg), 2)).Average();
                var maxVariance = Math.Pow(0.25, 2); // heurístico
                summary.Muscles.BalanceIndex = (decimal)Math.Max(0, 1 - variance / maxVariance);
                if (summary.Muscles.Dominant.Any()) summary.Muscles.Alerts.Add("Predominio marcado en: " + string.Join(", ", summary.Muscles.Dominant));
            }
        }

        private void BuildComparison(ProgressSummaryResponse summary, IReadOnlyList<Daily> dailies, DateTime startLocal, DateTime endLocal)
        {
            var sessions = dailies.Where(d => d.Percentage > 0).ToList();
            if (!sessions.Any()) return;
            var mid = startLocal.AddDays(((endLocal - startLocal).Days + 1) / 2);
            var first = sessions.Where(s => s.CreatedAt.ToUniversalTime() < mid).ToList();
            var second = sessions.Where(s => s.CreatedAt.ToUniversalTime() >= mid).ToList();
            SimpleWindowStats Map(List<Daily> list) => new SimpleWindowStats
            {
                Sessions = list.Count,
                AvgCompletion = list.Count > 0 ? (decimal)Math.Round(list.Average(x => x.Percentage), 2) : 0,
                TotalMinutes = list.Sum(x => (int)(x.EndDate - x.StartDate).TotalMinutes),
                DistinctExercises = 0 // pendiente hasta incluir ejercicios
            };
            summary.Comparison = new ComparisonBlock
            {
                FirstHalf = Map(first),
                SecondHalf = Map(second),
                Trend = (Map(second).AvgCompletion > Map(first).AvgCompletion) ? "increasing" : (Map(second).AvgCompletion < Map(first).AvgCompletion ? "decreasing" : "stable")
            };
        }

        private async Task BuildAssessmentsAsync(ProgressSummaryResponse summary, Guid userId, DateTime startUtc)
        {
            try
            {
                var all = await _physicalAssessmentRepository.GetAllPhysicalAssessmentsAsync();
                var user = all.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).ToList();
                if (!user.Any()) return;
                var latest = user.First();
                var baseline = user.Where(a => a.CreatedAt <= startUtc).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                var latestDict = new Dictionary<string, string?>
                {
                    [nameof(latest.Weight)] = latest.Weight,
                    [nameof(latest.BodyFatPercentage)] = latest.BodyFatPercentage,
                    [nameof(latest.MuscleMass)] = latest.MuscleMass,
                    [nameof(latest.Waist)] = latest.Waist,
                    [nameof(latest.Hips)] = latest.Hips,
                    ["Chest"] = latest.Chest
                };
                summary.Assessments = new AssessmentBlock { Latest = latestDict };
                if (baseline != null)
                {
                    foreach (var kv in latestDict)
                    {
                        var baselineVal = baseline.GetType().GetProperty(kv.Key)?.GetValue(baseline)?.ToString();
                        if (baselineVal != kv.Value && (baselineVal != null || kv.Value != null))
                        {
                            summary.Assessments.Changes.Add(new AssessmentChange { Field = kv.Key, OldValue = baselineVal, NewValue = kv.Value });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProgressReport] No fue posible construir bloque de valoraciones físicas");
            }
        }

        private void BuildSuggestions(ProgressSummaryResponse summary)
        {
            if (summary.Adherence.AdherencePct < 60) summary.Suggestions.Add("Mejorar adherencia: intenta completar más días planificados.");
            if (summary.Execution.AvgCompletion < 70 && summary.Adherence.CompletedDays > 3) summary.Suggestions.Add("Revisar intensidad o duración: el porcentaje promedio es bajo.");
            if (summary.Discipline.ConsistencyIndex < 0.4m) summary.Suggestions.Add("Regulariza tu agenda de entrenamiento para mejorar consistencia.");
        }

        public async Task<ApplicationResponse<ExercisesDetailResponse>> GetExercisesDetailAsync(ProgressReportRequest request)
        {
            var summary = await GetSummaryAsync(request);
            if (!summary.Success || summary.Data == null)
                return new ApplicationResponse<ExercisesDetailResponse> { Success = false, Message = summary.Message };
            var data = summary.Data;
            return new ApplicationResponse<ExercisesDetailResponse>
            {
                Success = true,
                Data = new ExercisesDetailResponse
                {
                    From = data.Period.From,
                    To = data.Period.To,
                    DistinctExercises = data.Exercises.DistinctExercises,
                    Exercises = data.Exercises.TopExercises, // se podría expandir a todos con otra lógica
                    TotalSeries = data.Exercises.TotalSeries,
                    TotalReps = data.Exercises.TotalReps
                }
            };
        }

        public async Task<ApplicationResponse<ObjectivesDetailResponse>> GetObjectivesDetailAsync(ProgressReportRequest request)
        {
            var summary = await GetSummaryAsync(request);
            if (!summary.Success || summary.Data == null)
                return new ApplicationResponse<ObjectivesDetailResponse> { Success = false, Message = summary.Message };
            var data = summary.Data;
            return new ApplicationResponse<ObjectivesDetailResponse>
            {
                Success = true,
                Data = new ObjectivesDetailResponse
                {
                    From = data.Period.From,
                    To = data.Period.To,
                    Planned = data.Objectives.Planned,
                    Executed = data.Objectives.Executed,
                    Gaps = data.Objectives.Gaps
                }
            };
        }

        public async Task<ApplicationResponse<MusclesDetailResponse>> GetMusclesDetailAsync(ProgressReportRequest request)
        {
            var summary = await GetSummaryAsync(request);
            if (!summary.Success || summary.Data == null)
                return new ApplicationResponse<MusclesDetailResponse> { Success = false, Message = summary.Message };
            var data = summary.Data;
            return new ApplicationResponse<MusclesDetailResponse>
            {
                Success = true,
                Data = new MusclesDetailResponse
                {
                    From = data.Period.From,
                    To = data.Period.To,
                    Distribution = data.Muscles.Distribution,
                    Dominant = data.Muscles.Dominant,
                    Underworked = data.Muscles.Underworked,
                    BalanceIndex = data.Muscles.BalanceIndex,
                    Alerts = data.Muscles.Alerts
                }
            };
        }

        public async Task<ApplicationResponse<SuggestionsResponse>> GetSuggestionsAsync(ProgressReportRequest request)
        {
            var summary = await GetSummaryAsync(request);
            if (!summary.Success || summary.Data == null)
                return new ApplicationResponse<SuggestionsResponse> { Success = false, Message = summary.Message };
            return new ApplicationResponse<SuggestionsResponse>
            {
                Success = true,
                Data = new SuggestionsResponse
                {
                    From = summary.Data.Period.From,
                    To = summary.Data.Period.To,
                    Suggestions = summary.Data.Suggestions
                }
            };
        }

        public async Task<ApplicationResponse<DisciplineDetailResponse>> GetDisciplineDetailAsync(ProgressReportRequest request)
        {
            var summary = await GetSummaryAsync(request);
            if (!summary.Success || summary.Data == null)
                return new ApplicationResponse<DisciplineDetailResponse> { Success = false, Message = summary.Message };
            var s = summary.Data;
            var resp = new DisciplineDetailResponse
            {
                From = s.Period.From,
                To = s.Period.To,
                ConsistencyIndex = s.Discipline.ConsistencyIndex,
                CommonStartHour = s.Discipline.CommonStartHour,
                ScheduleRegularity = s.Discipline.ScheduleRegularity,
                CurrentStreak = s.Adherence.CurrentStreak,
                MaxStreak = s.Adherence.MaxStreak,
                AdherencePct = s.Adherence.AdherencePct
            };
            resp.Weekdays.AddRange(s.Adherence.ByWeekday);
            return new ApplicationResponse<DisciplineDetailResponse> { Success = true, Data = resp };
        }

        public async Task<ApplicationResponse<List<ProgressSummaryResponse>>> GetMultiSummaryAsync(MultiProgressReportRequest request)
        {
            try
            {
                if (request.Periods == null || request.Periods.Count == 0)
                    return new ApplicationResponse<List<ProgressSummaryResponse>> { Success = false, Message = "Debe especificar al menos un periodo." };

                var list = new List<ProgressSummaryResponse>();
                foreach (var pr in request.Periods)
                {
                    var single = new ProgressReportRequest
                    {
                        UserId = request.UserId,
                        StartDate = pr.From,
                        EndDate = pr.To,
                        Timezone = request.Timezone,
                        IncludeAssessments = request.IncludeAssessments,
                        ComparePreviousPeriod = request.ComparePreviousPeriod,
                        MinCompletionForAdherence = request.MinCompletionForAdherence,
                        TopExercises = request.TopExercises
                    };
                    var resp = await GetSummaryAsync(single);
                    if (resp.Success && resp.Data != null)
                        list.Add(resp.Data);
                }
                return new ApplicationResponse<List<ProgressSummaryResponse>> { Success = true, Data = list };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                _logger.LogError(ex, "[ProgressReport] Error generando multi resumen");
                return new ApplicationResponse<List<ProgressSummaryResponse>> { Success = false, Message = "Error técnico al generar multi resumen." };
            }
        }

        public async Task<ApplicationResponse<MultiProgressHistoryResponse>> GetMultiSummaryWithHistoryAsync(MultiProgressReportRequest request)
        {
            var multi = await GetMultiSummaryAsync(request);
            if (!multi.Success || multi.Data == null)
                return new ApplicationResponse<MultiProgressHistoryResponse> { Success = false, Message = multi.Message };
            var history = new MultiProgressHistoryResponse { Periods = multi.Data };
            foreach (var p in multi.Data)
            {
                history.History.Add(new HistoryPoint
                {
                    PeriodFrom = p.Period.From,
                    PeriodTo = p.Period.To,
                    AdherencePct = p.Adherence.AdherencePct,
                    Sessions = p.Adherence.Sessions,
                    CompletedDays = p.Adherence.CompletedDays,
                    TargetDays = p.Adherence.TargetDays,
                    TotalMinutes = p.Time.TotalMinutes,
                    AvgCompletion = p.Execution.AvgCompletion,
                    BalanceIndex = p.Muscles.BalanceIndex
                });
            }
            return new ApplicationResponse<MultiProgressHistoryResponse> { Success = true, Data = history };
        }
    }
}
