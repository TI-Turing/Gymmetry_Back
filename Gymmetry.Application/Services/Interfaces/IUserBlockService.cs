using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserBlock;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IUserBlockService
    {
        Task<ApplicationResponse<UserBlockResponse>> BlockUserAsync(UserBlockCreateRequest request, Guid blockerId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<bool>> UnblockUserAsync(Guid blockerId, Guid blockedUserId);
        Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetBlockedUsersAsync(Guid userId, int page = 1, int pageSize = 50);
        Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetBlockersAsync(Guid userId, int page = 1, int pageSize = 50);
        Task<ApplicationResponse<UserBlockCheckResponse>> CheckBlockStatusAsync(Guid userId, Guid targetUserId);
        Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<UserBlockStatsResponse>> GetStatsAsync(Guid userId);
        Task<ApplicationResponse<BulkUnblockResponse>> BulkUnblockAsync(BulkUnblockRequest request, Guid userId);
        Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetMutualBlocksAsync(Guid userId, int page = 1, int pageSize = 50);
    }
}