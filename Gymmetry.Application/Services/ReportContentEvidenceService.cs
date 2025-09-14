using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class ReportContentEvidenceService : IReportContentEvidenceService
    {
        private readonly ILogErrorService _logErrorService;
        private readonly IReportContentRepository _reportRepo;
        private readonly IBlobStorageService _blob;
        private const string EvidenceContainer = "report-evidence";

        public ReportContentEvidenceService(ILogErrorService logErrorService, IReportContentRepository reportRepo, IBlobStorageService blob)
        {
            _logErrorService = logErrorService;
            _reportRepo = reportRepo;
            _blob = blob;
        }

        public async Task<IReadOnlyList<string>> UploadEvidenceAsync(Guid reportId, IEnumerable<(string FileName, byte[] Content, string ContentType)> files)
        {
            var stored = new List<string>();
            try
            {
                foreach (var f in files)
                {
                    var blobPath = $"{reportId}/{Guid.NewGuid()}_{f.FileName}";
                    var uri = await _blob.UploadAsync(EvidenceContainer, blobPath, f.Content, string.IsNullOrWhiteSpace(f.ContentType)?"application/octet-stream":f.ContentType);
                    var evidence = new ReportContentEvidence
                    {
                        ReportContentId = reportId,
                        FileName = f.FileName,
                        ContentType = f.ContentType,
                        StoragePath = uri,
                        SizeBytes = f.Content.LongLength
                    };
                    await _reportRepo.AddEvidenceAsync(evidence);
                    stored.Add(uri);
                }
                var manifestJson = System.Text.Json.JsonSerializer.Serialize(stored);
                await _blob.UploadTextAsync(EvidenceContainer, $"{reportId}/manifest.json", manifestJson);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
            }
            return stored;
        }

        public Task<IReadOnlyList<ReportContentEvidence>> GetEvidenceAsync(Guid reportId)
            => _reportRepo.GetEvidenceAsync(reportId).ContinueWith(t => (IReadOnlyList<ReportContentEvidence>)t.Result);

        public async Task<bool> DeleteEvidenceAsync(Guid reportId, Guid evidenceId)
        {
            try
            {
                var list = await _reportRepo.GetEvidenceAsync(reportId);
                var ev = list.FirstOrDefault(e => e.Id == evidenceId);
                var ok = await _reportRepo.DeleteEvidenceAsync(evidenceId, reportId);
                if (ok && ev != null && !string.IsNullOrWhiteSpace(ev.StoragePath) && Uri.TryCreate(ev.StoragePath, UriKind.Absolute, out var uri))
                {
                    var trimmed = uri.AbsolutePath.Trim('/');
                    var firstSlash = trimmed.IndexOf('/');
                    if (firstSlash > 0)
                    {
                        var container = trimmed.Substring(0, firstSlash);
                        var path = trimmed.Substring(firstSlash + 1);
                        if (string.Equals(container, EvidenceContainer, StringComparison.OrdinalIgnoreCase))
                        {
                            await _blob.DeleteIfExistsAsync(EvidenceContainer, path);
                        }
                    }
                }
                return ok;
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return false;
            }
        }
    }
}
