using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    // Placeholder if needed at application layer (current repository interface lives in Repository project)
    public interface IDailyExerciseRepositoryAdapter
    {
        Task<IEnumerable<DailyExercise>> GetByDailyIdsAsync(IEnumerable<System.Guid> dailyIds);
    }
}
