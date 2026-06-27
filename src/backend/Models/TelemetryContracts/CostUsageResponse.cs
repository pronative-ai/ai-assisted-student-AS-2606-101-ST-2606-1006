namespace Backend.Models.TelemetryContracts;

public class CostUsageResponse
{
    public DateTime WindowStartUtc { get; set; }
    public DateTime WindowEndUtc { get; set; }
    public string StudentContext { get; set; } = "student-default";
    public string MetricName { get; set; } = "opencode.cost.usage";
    public string AggregationStatus { get; set; } = "complete";
    public double? UsageDelta { get; set; }
    public double? RatePerHour { get; set; }
    public DateTime? BaselineSampleTimestampUtc { get; set; }
    public double? BaselineSampleValue { get; set; }
    public DateTime? EndingSampleTimestampUtc { get; set; }
    public double? EndingSampleValue { get; set; }
    public string? NotesOrReason { get; set; }
}
