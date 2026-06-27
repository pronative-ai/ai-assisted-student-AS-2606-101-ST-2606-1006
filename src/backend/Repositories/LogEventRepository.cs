using Backend.Data;
using Backend.Models.TelemetryContracts;
using Microsoft.Azure.Cosmos;

namespace Backend.Repositories;

public class LogEventRepository : ILogEventRepository
{
    private readonly Container _container;

    public LogEventRepository(CosmosDbContext context)
    {
        _container = context.LogsContainer;
    }

    public async Task SaveAsync(LogEvent logEvent)
    {
        await _container.CreateItemAsync(logEvent, new PartitionKey(logEvent.PartitionKey));
    }
}
