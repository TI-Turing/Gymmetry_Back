using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Repository.Services
{
    public class PostShareRepository : IPostShareRepository
    {
        private readonly GymmetryContext _context;
        private readonly ILogger<PostShareRepository> _logger;

        public PostShareRepository(GymmetryContext context, ILogger<PostShareRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PostShare> CreatePostShareAsync(PostShare postShare)
        {
            postShare.Id = Guid.NewGuid();
            postShare.CreatedAt = DateTime.UtcNow;
            postShare.IsActive = true;

            _context.PostShares.Add(postShare);
            await _context.SaveChangesAsync();
            return postShare;
        }

        public async Task<bool> UpdatePostShareAsync(PostShare postShare)
        {
            var existing = await _context.PostShares
                .FirstOrDefaultAsync(p => p.Id == postShare.Id && p.IsActive);
            
            if (existing == null) return false;

            existing.Metadata = postShare.Metadata ?? existing.Metadata;
            existing.IsActive = postShare.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostShareAsync(Guid id)
        {
            var postShare = await _context.PostShares
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            
            if (postShare == null) return false;

            postShare.IsActive = false;
            postShare.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PostShare?> GetPostShareByIdAsync(Guid id)
        {
            return await _context.PostShares
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<IEnumerable<PostShare>> FindPostSharesByFieldsAsync(Dictionary<string, object> filters)
        {
            var query = _context.PostShares.AsQueryable();

            // Aplicar filtros dinámicos
            foreach (var filter in filters)
            {
                switch (filter.Key.ToLower())
                {
                    case "postid":
                        if (filter.Value is Guid postId)
                            query = query.Where(p => p.PostId == postId);
                        break;
                    case "sharedby":
                        if (filter.Value is Guid sharedBy)
                            query = query.Where(p => p.SharedBy == sharedBy);
                        break;
                    case "sharedwith":
                        if (filter.Value is Guid sharedWith)
                            query = query.Where(p => p.SharedWith == sharedWith);
                        break;
                    case "sharetype":
                        if (filter.Value is string shareType)
                            query = query.Where(p => p.ShareType == shareType);
                        break;
                    case "platform":
                        if (filter.Value is string platform)
                            query = query.Where(p => p.Platform == platform);
                        break;
                    case "datefrom":
                        if (filter.Value is DateTime dateFrom)
                            query = query.Where(p => p.CreatedAt >= dateFrom);
                        break;
                    case "dateto":
                        if (filter.Value is DateTime dateTo)
                            query = query.Where(p => p.CreatedAt <= dateTo);
                        break;
                    case "isactive":
                        if (filter.Value is bool isActive)
                            query = query.Where(p => p.IsActive == isActive);
                        break;
                }
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Take(1000) // Límite de seguridad
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetPostShareCountersByPostIdAsync(Guid postId)
        {
            var shares = await _context.PostShares
                .Where(p => p.PostId == postId && p.IsActive)
                .GroupBy(p => p.Platform)
                .Select(g => new { Platform = g.Key, Count = g.Count() })
                .ToListAsync();

            var counters = new Dictionary<string, int>
            {
                ["WhatsApp"] = 0,
                ["Instagram"] = 0,
                ["Facebook"] = 0,
                ["Twitter"] = 0,
                ["SMS"] = 0,
                ["Email"] = 0,
                ["Other"] = 0,
                ["App"] = 0
            };

            foreach (var share in shares)
            {
                if (counters.ContainsKey(share.Platform))
                    counters[share.Platform] = share.Count;
            }

            return counters;
        }

        public async Task<bool> ExistsAsync(Guid postId, Guid sharedBy, string platform, string shareType)
        {
            return await _context.PostShares
                .AnyAsync(p => p.PostId == postId && 
                             p.SharedBy == sharedBy && 
                             p.Platform == platform && 
                             p.ShareType == shareType && 
                             p.IsActive);
        }
    }
}