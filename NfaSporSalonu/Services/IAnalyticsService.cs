using NfaSporSalonu.Models.DTOs;

namespace NfaSporSalonu.Services;

public interface IAnalyticsService
{
    Task<ProgressReportDto> GetProgressReport(int userId);
    Task<AnalyticsTrendsResponse> GetMeasurementTrends(int userId);
}
