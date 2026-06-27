using Backend.Models.TelemetryContracts;
using Backend.Repositories;

namespace Backend.Services;

public class CostAggregationService
{
    private readonly IMetricSampleRepository _metricRepo;

    public CostAggregationService(IMetricSampleRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<CostUsageResponse> GetCostUsageAsync(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return new CostUsageResponse
            {
                WindowStartUtc = start,
                WindowEndUtc = end,
                AggregationStatus = "invalid_range",
                NotesOrReason = "Start must be earlier than end"
            };
        }

        var windowDurationHours = (end - start).TotalHours;

        var baseline = await _metricRepo.GetLatestSampleAtOrBeforeAsync(start);
        var ending = await _metricRepo.GetLatestSampleAtOrBeforeAsync(end);

        var response = new CostUsageResponse
        {
            WindowStartUtc = start,
            WindowEndUtc = end,
            MetricName = "opencode.cost.usage"
        };

        if (baseline == null && ending == null)
        {
            response.AggregationStatus = "no_samples_in_window";
            response.UsageDelta = 0;
            response.RatePerHour = 0;
            response.NotesOrReason = "No telemetry samples found in or before the requested window";
            return response;
        }

        if (baseline == null)
        {
            response.AggregationStatus = "incomplete_missing_baseline";
            response.EndingSampleTimestampUtc = ending!.SampleTimestampUtc;
            response.EndingSampleValue = ending.CumulativeValue;
            response.NotesOrReason = "No baseline sample at or before window start; delta cannot be computed";
            return response;
        }

        if (ending == null)
        {
            response.AggregationStatus = "incomplete_missing_baseline";
            response.BaselineSampleTimestampUtc = baseline.SampleTimestampUtc;
            response.BaselineSampleValue = baseline.CumulativeValue;
            response.NotesOrReason = "No ending sample at or before window end";
            return response;
        }

        response.BaselineSampleTimestampUtc = baseline.SampleTimestampUtc;
        response.BaselineSampleValue = baseline.CumulativeValue;
        response.EndingSampleTimestampUtc = ending.SampleTimestampUtc;
        response.EndingSampleValue = ending.CumulativeValue;

        if (ending.CumulativeValue < baseline.CumulativeValue)
        {
            response.AggregationStatus = "anomaly_counter_decrease";
            response.NotesOrReason = $"Cumulative counter decreased from {baseline.CumulativeValue} to {ending.CumulativeValue}; delta not computed";
            return response;
        }

        var delta = ending.CumulativeValue - baseline.CumulativeValue;
        response.AggregationStatus = "complete";
        response.UsageDelta = delta;
        response.RatePerHour = windowDurationHours > 0 ? Math.Round(delta / windowDurationHours, 6) : 0;

        return response;
    }
}
