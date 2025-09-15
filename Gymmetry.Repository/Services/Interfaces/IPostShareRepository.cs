using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IPostShareRepository
    {
        Task<PostShare> CreatePostShareAsync(PostShare postShare);
        Task<bool> UpdatePostShareAsync(PostShare postShare);
        Task<bool> DeletePostShareAsync(Guid id);
        Task<PostShare?> GetPostShareByIdAsync(Guid id);
        Task<IEnumerable<PostShare>> FindPostSharesByFieldsAsync(Dictionary<string, object> filters);
        Task<Dictionary<string, int>> GetPostShareCountersByPostIdAsync(Guid postId);
        Task<bool> ExistsAsync(Guid postId, Guid sharedBy, string platform, string shareType);
    }
}