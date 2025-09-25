using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<UserBlockResponse> Items { get; set; } = new();
    }
}