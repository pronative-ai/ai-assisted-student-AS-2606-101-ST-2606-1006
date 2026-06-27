using Backend.Repositories;
using Backend.Services;

namespace Backend.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this WebApplication app)
    {
        app.MapPost("/v1/metrics", async (HttpRequest request, OtlpIngestionService ingestionService, IMetricSampleRepository metricRepo) =>
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return Results.BadRequest(new { error = "Empty request body", accepted = 0, rejected = 0 });
            }

            var (accepted, _, rejected) = ingestionService.ProcessMetricsPayload(body, "otlp_http_protobuf");

            foreach (var sample in accepted)
            {
                await metricRepo.SaveAsync(sample);
            }

            return Results.Ok(new
            {
                message = "Metrics processed",
                accepted = accepted.Count,
                rejected
            });
        });

        app.MapPost("/v1/logs", async (HttpRequest request, OtlpIngestionService ingestionService, ILogEventRepository logRepo) =>
        {
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return Results.BadRequest(new { error = "Empty request body", accepted = 0, rejected = 0 });
            }

            var (_, accepted, rejected) = ingestionService.ProcessLogsPayload(body, "otlp_http_protobuf");

            foreach (var logEvent in accepted)
            {
                await logRepo.SaveAsync(logEvent);
            }

            return Results.Ok(new
            {
                message = "Logs processed",
                accepted = accepted.Count,
                rejected
            });
        });
    }
}
