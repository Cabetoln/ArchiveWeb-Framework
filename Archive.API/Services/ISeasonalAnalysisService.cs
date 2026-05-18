using Archive.API.DTOs;

namespace Archive.API.Services;

public interface ISeasonalAnalysisService
{
    Task<SeasonalInsightResponse?> GetInsightAsync(Guid itemId);
    Task<SeasonalAnalysisStatusResponse> GetPendingStatusAsync();
    Task<SeasonalAnalysisStatusResponse> ProcessPendingAnalysisAsync();
}
