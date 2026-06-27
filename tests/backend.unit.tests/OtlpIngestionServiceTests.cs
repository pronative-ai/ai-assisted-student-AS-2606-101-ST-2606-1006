using Backend.Services;
using Xunit;

namespace Backend.Tests;

public class OtlpIngestionServiceTests
{
    private readonly OtlpIngestionService _sut = new();

    [Fact]
    public void ProcessMetricsPayload_AcceptsValidOpenCodeCostUsage()
    {
        var body = "opencode.cost.usage|1.25|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Single(metrics);
        Assert.Equal(0, rejected);
        Assert.Equal("opencode.cost.usage", metrics[0].SignalName);
        Assert.Equal(1.25, metrics[0].CumulativeValue);
    }

    [Fact]
    public void ProcessMetricsPayload_RejectsUnsupportedMetric()
    {
        var body = "unsupported.metric|1.25|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Empty(metrics);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessMetricsPayload_RejectsNegativeCost()
    {
        var body = "opencode.cost.usage|-1.25|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Empty(metrics);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessMetricsPayload_RejectsNonNumericCost()
    {
        var body = "opencode.cost.usage|abc|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Empty(metrics);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessMetricsPayload_RejectsInvalidTimestamp()
    {
        var body = "opencode.cost.usage|1.25|not-a-date";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Empty(metrics);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessLogsPayload_AcceptsValidApiRequest()
    {
        var body = "api_request|2026-01-01T10:00:00Z|\"GET /health\"|\"method=GET\"";
        var (metrics, logs, rejected) = _sut.ProcessLogsPayload(body, "otlp_http_protobuf");

        Assert.Single(logs);
        Assert.Equal(0, rejected);
        Assert.Equal("api_request", logs[0].EventName);
    }

    [Fact]
    public void ProcessLogsPayload_AcceptsValidApiError()
    {
        var body = "api_error|2026-01-01T10:00:00Z|\"500 Internal\"|\"error=timeout\"";
        var (metrics, logs, rejected) = _sut.ProcessLogsPayload(body, "otlp_http_protobuf");

        Assert.Single(logs);
        Assert.Equal(0, rejected);
        Assert.Equal("api_error", logs[0].EventName);
    }

    [Fact]
    public void ProcessLogsPayload_RejectsUnsupportedLogEvent()
    {
        var body = "unsupported.event|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessLogsPayload(body, "otlp_http_protobuf");

        Assert.Empty(logs);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessMetricsPayload_MixedSupportedAndUnsupported_ProcessesOnlySupported()
    {
        var body = "opencode.cost.usage|1.25|2026-01-01T10:00:00Z\nunsupported.metric|2.00|2026-01-01T10:00:00Z";
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload(body, "otlp_http_protobuf");

        Assert.Single(metrics);
        Assert.Equal(1.25, metrics[0].CumulativeValue);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public void ProcessMetricsPayload_EmptyBody_ReturnsEmptyResults()
    {
        var (metrics, logs, rejected) = _sut.ProcessMetricsPayload("", "otlp_http_protobuf");

        Assert.Empty(metrics);
        Assert.Equal(0, rejected);
    }
}
