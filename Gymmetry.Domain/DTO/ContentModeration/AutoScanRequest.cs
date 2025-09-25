using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class AutoScanRequest : ApiRequest
    {
        public ContentType? ContentType { get; set; }
        public DateTime? SinceDate { get; set; }
        public int? LimitItems { get; set; } = 100;
    }
}