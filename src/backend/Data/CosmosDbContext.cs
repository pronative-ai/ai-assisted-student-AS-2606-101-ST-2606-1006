using Microsoft.Azure.Cosmos;

namespace Backend.Data;

public class CosmosDbContext
{
    private readonly Container _metricsContainer;
    private readonly Container _logsContainer;

    public CosmosDbContext(string connectionString, string databaseName, string metricsContainerName, string logsContainerName)
    {
        var client = new CosmosClient(connectionString);
        _metricsContainer = client.GetContainer(databaseName, metricsContainerName);
        _logsContainer = client.GetContainer(databaseName, logsContainerName);
    }

    public Container MetricsContainer => _metricsContainer;
    public Container LogsContainer => _logsContainer;
}
