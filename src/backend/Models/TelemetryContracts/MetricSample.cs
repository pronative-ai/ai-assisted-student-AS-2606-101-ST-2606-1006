namespace Backend.Models.TelemetryContracts;

public class MetricSample
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartitionKey { get; set; } = "student-default";
    public string StudentContext { get; set; } = "student-default";
    public string SignalKind { get; set; } = "metric";
    public string SignalName { get; set; } = "opencode.cost.usage";
    public DateTime SampleTimestampUtc { get; set; }
    public double CumulativeValue { get; set; }
    public string? ResourceAttributes { get; set; }
    public string? MetricAttributes { get; set; }
    public string SourceTransport { get; set; } = "otlp_http_protobuf";
    public DateTime IngestedAtUtc { get; set; } = DateTime.UtcNow;
}
