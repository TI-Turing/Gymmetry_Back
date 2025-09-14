using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IUserBlockRepository
    {
        Task<UserBlock> CreateAsync(UserBlock userBlock);
        Task<UserBlock?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid blockerId, Guid blockedUserId);
        Task<bool> ExistsAsync(Guid blockerId, Guid blockedUserId);
        Task<IEnumerable<UserBlock>> GetBlockedByUserAsync(Guid blockerId, int page = 1, int pageSize = 50);
        Task<IEnumerable<UserBlock>> GetBlockersOfUserAsync(Guid blockedUserId, int page = 1, int pageSize = 50);
        Task<IEnumerable<UserBlock>> GetMutualBlocksAsync(Guid userId, int page = 1, int pageSize = 50);
        Task<IEnumerable<UserBlock>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<UserBlock?> GetBlockBetweenUsersAsync(Guid user1Id, Guid user2Id);
        Task<int> CountBlocksByUserTodayAsync(Guid blockerId);
        Task<int> CountTotalBlocksAsync();
        Task<int> CountBlockedByUserAsync(Guid blockerId);
        Task<int> CountBlockersOfUserAsync(Guid blockedUserId);
        Task<int> CountMutualBlocksAsync(Guid userId);
        Task<Dictionary<string, int>> GetBlocksStatsLast7DaysAsync(Guid userId);
        Task<bool> BulkUnblockAsync(Guid blockerId, IEnumerable<Guid> blockedUserIds);
        Task CleanupInteractionsAsync(Guid blockerId, Guid blockedUserId);
        Task<bool> IsUserBlockedByMeAsync(Guid myUserId, Guid targetUserId);
        Task<bool> AmIBlockedByUserAsync(Guid myUserId, Guid targetUserId);
        Task<IEnumerable<Guid>> GetBlockedUserIdsAsync(Guid blockerId);
        Task<IEnumerable<Guid>> GetBlockerUserIdsAsync(Guid blockedUserId);
    }
}