namespace Backend.Models.TelemetryContracts;

public class CostTrendResponse
{
    public DateTime WindowStartUtc { get; set; }
    public DateTime WindowEndUtc { get; set; }
    public int BucketGranularityHours { get; set; }
    public string StudentContext { get; set; } = "student-default";
    public string MetricName { get; set; } = "opencode.cost.usage";
    public List<CostTrendBucket> Buckets { get; set; } = [];
}

public class CostTrendBucket
{
    public DateTime BucketStartUtc { get; set; }
    public DateTime BucketEndUtc { get; set; }
    public double CostValue { get; set; }
}
