using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IReportContentEvidenceService
    {
        Task<IReadOnlyList<string>> UploadEvidenceAsync(Guid reportId, IEnumerable<(string FileName, byte[] Content, string ContentType)> files);
        Task<IReadOnlyList<ReportContentEvidence>> GetEvidenceAsync(Guid reportId);
        Task<bool> DeleteEvidenceAsync(Guid reportId, Guid evidenceId);
    }
}
