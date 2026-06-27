using Backend.Data;
using Backend.Models.TelemetryContracts;
using Microsoft.Azure.Cosmos;

namespace Backend.Repositories;

public class MetricSampleRepository : IMetricSampleRepository
{
    private readonly Container _container;

    public MetricSampleRepository(CosmosDbContext context)
    {
        _container = context.MetricsContainer;
    }

    public async Task SaveAsync(MetricSample sample)
    {
        await _container.CreateItemAsync(sample, new PartitionKey(sample.PartitionKey));
    }

    public async Task<MetricSample?> GetLatestSampleAtOrBeforeAsync(DateTime timestamp)
    {
        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.SampleTimestampUtc <= @timestamp AND c.SignalName = @signalName ORDER BY c.SampleTimestampUtc DESC")
            .WithParameter("@timestamp", timestamp)
            .WithParameter("@signalName", "opencode.cost.usage");

        using var iterator = _container.GetItemQueryIterator<MetricSample>(query);
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<IReadOnlyList<MetricSample>> GetSamplesInRangeAsync(DateTime start, DateTime end)
    {
        var results = new List<MetricSample>();
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.SampleTimestampUtc >= @start AND c.SampleTimestampUtc <= @end AND c.SignalName = @signalName ORDER BY c.SampleTimestampUtc ASC")
            .WithParameter("@start", start)
            .WithParameter("@end", end)
            .WithParameter("@signalName", "opencode.cost.usage");

        using var iterator = _container.GetItemQueryIterator<MetricSample>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}
