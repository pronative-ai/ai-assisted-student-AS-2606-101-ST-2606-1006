using Backend.Models.TelemetryContracts;

namespace Backend.Repositories;

public interface IMetricSampleRepository
{
    Task SaveAsync(MetricSample sample);
    Task<MetricSample?> GetLatestSampleAtOrBeforeAsync(DateTime timestamp);
    Task<IReadOnlyList<MetricSample>> GetSamplesInRangeAsync(DateTime start, DateTime end);
}
