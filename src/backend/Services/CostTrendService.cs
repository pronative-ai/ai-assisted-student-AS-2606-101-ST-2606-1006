using Backend.Models.TelemetryContracts;
using Backend.Repositories;

namespace Backend.Services;

public class CostTrendService
{
    private readonly IMetricSampleRepository _metricRepo;

    public CostTrendService(IMetricSampleRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<CostTrendResponse> GetTrendAsync(DateTime start, DateTime end)
    {
        var bucketGranularityHours = 24;
        var samples = await _metricRepo.GetSamplesInRangeAsync(start, end);

        var buckets = new List<CostTrendBucket>();
        var bucketStart = start;

        while (bucketStart < end)
        {
            var bucketEnd = bucketStart.AddHours(bucketGranularityHours);
            if (bucketEnd > end) bucketEnd = end;

            var bucketSamples = samples
                .Where(s => s.SampleTimestampUtc >= bucketStart && s.SampleTimestampUtc < bucketEnd)
                .ToList();

            double totalCost = 0;
            if (bucketSamples.Count > 0)
            {
                var ordered = bucketSamples.OrderBy(s => s.SampleTimestampUtc).ToList();
                var first = ordered.First().CumulativeValue;
                var last = ordered.Last().CumulativeValue;

                if (last >= first)
                {
                    totalCost = last - first;
                }
            }

            buckets.Add(new CostTrendBucket
            {
                BucketStartUtc = bucketStart,
                BucketEndUtc = bucketEnd,
                CostValue = totalCost
            });

            bucketStart = bucketEnd;
        }

        return new CostTrendResponse
        {
            WindowStartUtc = start,
            WindowEndUtc = end,
            BucketGranularityHours = bucketGranularityHours,
            Buckets = buckets
        };
    }
}
