namespace Backend.Models.TelemetryContracts;

public class LogEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartitionKey { get; set; } = "student-default";
    public string StudentContext { get; set; } = "student-default";
    public string SignalKind { get; set; } = "log";
    public string EventName { get; set; } = string.Empty;
    public DateTime EventTimestampUtc { get; set; }
    public string? Body { get; set; }
    public string? Attributes { get; set; }
    public string? ResourceAttributes { get; set; }
    public string SourceTransport { get; set; } = "otlp_http_protobuf";
    public DateTime IngestedAtUtc { get; set; } = DateTime.UtcNow;
}
