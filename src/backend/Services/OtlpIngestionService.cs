using Backend.Models.TelemetryContracts;

namespace Backend.Services;

public class OtlpIngestionService
{
    private static readonly HashSet<string> SupportedMetrics = ["opencode.cost.usage"];
    private static readonly HashSet<string> SupportedLogEvents = ["api_request", "api_error"];

    public (List<MetricSample> acceptedMetrics, List<LogEvent> acceptedLogs, int rejectedCount)
        ProcessMetricsPayload(string body, string sourceTransport)
    {
        var acceptedMetrics = new List<MetricSample>();
        var rejectedCount = 0;

        try
        {
            var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length < 3)
                {
                    rejectedCount++;
                    continue;
                }

                var metricName = parts[0].Trim();
                if (!SupportedMetrics.Contains(metricName))
                {
                    rejectedCount++;
                    continue;
                }

                if (!double.TryParse(parts[1].Trim(), out var costValue) || costValue < 0)
                {
                    rejectedCount++;
                    continue;
                }

                if (!DateTime.TryParse(parts[2].Trim(), out var timestamp))
                {
                    rejectedCount++;
                    continue;
                }

                acceptedMetrics.Add(new MetricSample
                {
                    SignalName = metricName,
                    CumulativeValue = costValue,
                    SampleTimestampUtc = timestamp,
                    SourceTransport = sourceTransport,
                    ResourceAttributes = parts.Length > 3 ? parts[3].Trim() : null,
                    MetricAttributes = parts.Length > 4 ? parts[4].Trim() : null
                });
            }
        }
        catch
        {
            rejectedCount++;
        }

        return (acceptedMetrics, [], rejectedCount);
    }

    public (List<MetricSample> acceptedMetrics, List<LogEvent> acceptedLogs, int rejectedCount)
        ProcessLogsPayload(string body, string sourceTransport)
    {
        var acceptedLogs = new List<LogEvent>();
        var rejectedCount = 0;

        try
        {
            var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length < 2)
                {
                    rejectedCount++;
                    continue;
                }

                var eventName = parts[0].Trim();
                if (!SupportedLogEvents.Contains(eventName))
                {
                    rejectedCount++;
                    continue;
                }

                if (!DateTime.TryParse(parts[1].Trim(), out var timestamp))
                {
                    rejectedCount++;
                    continue;
                }

                acceptedLogs.Add(new LogEvent
                {
                    EventName = eventName,
                    EventTimestampUtc = timestamp,
                    SourceTransport = sourceTransport,
                    Body = parts.Length > 2 ? parts[2].Trim() : null,
                    Attributes = parts.Length > 3 ? parts[3].Trim() : null,
                    ResourceAttributes = parts.Length > 4 ? parts[4].Trim() : null
                });
            }
        }
        catch
        {
            rejectedCount++;
        }

        return ([], acceptedLogs, rejectedCount);
    }
}
