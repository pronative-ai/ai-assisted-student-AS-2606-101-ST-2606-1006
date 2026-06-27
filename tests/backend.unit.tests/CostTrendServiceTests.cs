using Backend.Models.TelemetryContracts;
using Backend.Repositories;
using Backend.Services;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CostTrendServiceTests
{
    private readonly Mock<IMetricSampleRepository> _mockRepo;
    private readonly CostTrendService _sut;

    public CostTrendServiceTests()
    {
        _mockRepo = new Mock<IMetricSampleRepository>();
        _sut = new CostTrendService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetTrendAsync_WithSamples_ReturnsBuckets()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

        var samples = new List<MetricSample>
        {
            new() { SampleTimestampUtc = new DateTime(2026, 1, 1, 6, 0, 0, DateTimeKind.Utc), CumulativeValue = 10.0 },
            new() { SampleTimestampUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc), CumulativeValue = 15.0 },
            new() { SampleTimestampUtc = new DateTime(2026, 1, 2, 6, 0, 0, DateTimeKind.Utc), CumulativeValue = 20.0 },
            new() { SampleTimestampUtc = new DateTime(2026, 1, 2, 18, 0, 0, DateTimeKind.Utc), CumulativeValue = 25.0 },
        };

        _mockRepo.Setup(r => r.GetSamplesInRangeAsync(start, end)).ReturnsAsync(samples);

        var result = await _sut.GetTrendAsync(start, end);

        Assert.Equal(2, result.Buckets.Count);
        Assert.Equal(5.0, result.Buckets[0].CostValue);
        Assert.Equal(5.0, result.Buckets[1].CostValue);
    }

    [Fact]
    public async Task GetTrendAsync_NoSamples_ReturnsZeroBuckets()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetSamplesInRangeAsync(start, end)).ReturnsAsync([]);

        var result = await _sut.GetTrendAsync(start, end);

        Assert.Equal(2, result.Buckets.Count);
        Assert.All(result.Buckets, b => Assert.Equal(0, b.CostValue));
    }

    [Fact]
    public async Task GetTrendAsync_SingleBucket_ReturnsOneBucket()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var samples = new List<MetricSample>
        {
            new() { SampleTimestampUtc = new DateTime(2026, 1, 1, 6, 0, 0, DateTimeKind.Utc), CumulativeValue = 10.0 },
        };

        _mockRepo.Setup(r => r.GetSamplesInRangeAsync(start, end)).ReturnsAsync(samples);

        var result = await _sut.GetTrendAsync(start, end);

        Assert.Single(result.Buckets);
        Assert.Equal(start, result.WindowStartUtc);
        Assert.Equal(end, result.WindowEndUtc);
    }
}
