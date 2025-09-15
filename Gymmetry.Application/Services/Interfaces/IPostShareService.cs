using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Request;
using Gymmetry.Domain.DTO.PostShare.Response;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IPostShareService
    {
        Task<ApplicationResponse<PostShareResponse>> CreatePostShareAsync(AddPostShareRequest request, string ip = "");
        Task<ApplicationResponse<bool>> UpdatePostShareAsync(UpdatePostShareRequest request);
        Task<ApplicationResponse<bool>> DeletePostShareAsync(Guid id);
        Task<ApplicationResponse<PostShareResponse?>> GetPostShareByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<PostShareResponse>>> FindPostSharesByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<PostShareCountersResponse>> GetPostShareCountersAsync(Guid postId);
    }
}