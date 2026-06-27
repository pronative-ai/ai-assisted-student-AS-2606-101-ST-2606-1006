using Backend.Services;

namespace Backend.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/opencode/cost-usage", async (string? start, string? end, CostAggregationService aggregationService) =>
        {
            if (!TryParseWindow(start, end, out var startUtc, out var endUtc, out var error))
                return error;

            var result = await aggregationService.GetCostUsageAsync(startUtc!.Value, endUtc!.Value);
            return Results.Ok(result);
        });

        app.MapGet("/api/opencode/cost-usage/trend", async (string? start, string? end, CostTrendService trendService) =>
        {
            if (!TryParseWindow(start, end, out var startUtc, out var endUtc, out var error))
                return error;

            var result = await trendService.GetTrendAsync(startUtc!.Value, endUtc!.Value);
            return Results.Ok(result);
        });
    }

    private static bool TryParseWindow(string? start, string? end, out DateTime? startUtc, out DateTime? endUtc, out IResult? error)
    {
        startUtc = null;
        endUtc = null;
        error = null;

        if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
        {
            error = Results.BadRequest(new
            {
                error = "Both 'start' and 'end' query parameters are required",
                expectedFormat = "ISO 8601 UTC (e.g., 2026-01-01T00:00:00Z)"
            });
            return false;
        }

        if (!DateTime.TryParse(start, out var s) || !DateTime.TryParse(end, out var e))
        {
            error = Results.BadRequest(new
            {
                error = "Invalid date format for 'start' or 'end'",
                expectedFormat = "ISO 8601 UTC (e.g., 2026-01-01T00:00:00Z)"
            });
            return false;
        }

        if (s.Kind == DateTimeKind.Unspecified) s = DateTime.SpecifyKind(s, DateTimeKind.Utc);
        if (e.Kind == DateTimeKind.Unspecified) e = DateTime.SpecifyKind(e, DateTimeKind.Utc);

        if (s >= e)
        {
            error = Results.BadRequest(new
            {
                error = "'start' must be earlier than 'end'"
            });
            return false;
        }

        startUtc = s;
        endUtc = e;
        return true;
    }
}
