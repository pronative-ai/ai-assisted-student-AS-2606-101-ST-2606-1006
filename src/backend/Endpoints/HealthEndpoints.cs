namespace Backend.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        }));

        app.MapGet("/ready", () => Results.Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow
        }));
    }
}
