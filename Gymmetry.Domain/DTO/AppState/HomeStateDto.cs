using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class HomeStateDto
    {
        public DisciplineDataDto Discipline { get; set; } = new();
        public PlanInfoDto PlanInfo { get; set; } = new();
        public TodayRoutineDto TodayRoutine { get; set; } = new();
        public DetailedProgressDto DetailedProgress { get; set; } = new();
    }
}