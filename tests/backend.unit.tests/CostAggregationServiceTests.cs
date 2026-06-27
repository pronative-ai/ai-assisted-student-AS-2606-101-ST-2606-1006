using Backend.Models.TelemetryContracts;
using Backend.Repositories;
using Backend.Services;
using Moq;
using Xunit;

namespace Backend.Tests;

public class CostAggregationServiceTests
{
    private readonly Mock<IMetricSampleRepository> _mockRepo;
    private readonly CostAggregationService _sut;

    public CostAggregationServiceTests()
    {
        _mockRepo = new Mock<IMetricSampleRepository>();
        _sut = new CostAggregationService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetCostUsageAsync_WithBaselineAndEnding_ReturnsDelta()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync(new MetricSample { CumulativeValue = 12.0, SampleTimestampUtc = start });
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync(new MetricSample { CumulativeValue = 18.5, SampleTimestampUtc = end });

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("complete", result.AggregationStatus);
        Assert.Equal(6.5, result.UsageDelta);
        Assert.Equal(6.5, result.RatePerHour);
    }

    [Fact]
    public async Task GetCostUsageAsync_NoBaseline_ReturnsIncompleteMissingBaseline()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync((MetricSample?)null);
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync(new MetricSample { CumulativeValue = 18.5, SampleTimestampUtc = end });

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("incomplete_missing_baseline", result.AggregationStatus);
        Assert.Null(result.UsageDelta);
    }

    [Fact]
    public async Task GetCostUsageAsync_NoSamplesAtAll_ReturnsNoSamplesInWindow()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync((MetricSample?)null);
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync((MetricSample?)null);

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("no_samples_in_window", result.AggregationStatus);
        Assert.Equal(0, result.UsageDelta);
    }

    [Fact]
    public async Task GetCostUsageAsync_CounterDecrease_ReturnsAnomaly()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync(new MetricSample { CumulativeValue = 18.5, SampleTimestampUtc = start });
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync(new MetricSample { CumulativeValue = 12.0, SampleTimestampUtc = end });

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("anomaly_counter_decrease", result.AggregationStatus);
        Assert.Null(result.UsageDelta);
    }

    [Fact]
    public async Task GetCostUsageAsync_StartEqualsEnd_ReturnsInvalidRange()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = start;

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("invalid_range", result.AggregationStatus);
    }

    [Fact]
    public async Task GetCostUsageAsync_MultipleEvents_ComputesCorrectDelta()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync(new MetricSample { CumulativeValue = 10.0, SampleTimestampUtc = start });
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync(new MetricSample { CumulativeValue = 20.0, SampleTimestampUtc = end });

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal("complete", result.AggregationStatus);
        Assert.Equal(10.0, result.UsageDelta);
    }

    [Fact]
    public async Task GetCostUsageAsync_RatePerHour_ComputedFromDeltaAndDuration()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(start))
            .ReturnsAsync(new MetricSample { CumulativeValue = 10.0, SampleTimestampUtc = start });
        _mockRepo.Setup(r => r.GetLatestSampleAtOrBeforeAsync(end))
            .ReturnsAsync(new MetricSample { CumulativeValue = 20.0, SampleTimestampUtc = end });

        var result = await _sut.GetCostUsageAsync(start, end);

        Assert.Equal(10.0, result.UsageDelta);
        Assert.Equal(5.0, result.RatePerHour);
    }
}
