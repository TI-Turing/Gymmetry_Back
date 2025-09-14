using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ReportContent
{
    public class ReportContentListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<ReportContentResponse> Items { get; set; } = new();
    }
}
