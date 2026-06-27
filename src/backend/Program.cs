using Backend.Data;
using Backend.Endpoints;
using Backend.Repositories;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var cosmosConnection = builder.Configuration.GetConnectionString("CosmosDb")
    ?? builder.Configuration["CosmosDb:ConnectionString"]
    ?? "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

var databaseName = builder.Configuration.GetValue<string>("CosmosDb:DatabaseName") ?? "opencode-telemetry";
var metricsContainer = builder.Configuration.GetValue<string>("CosmosDb:MetricsContainer") ?? "metric-samples";
var logsContainer = builder.Configuration.GetValue<string>("CosmosDb:LogsContainer") ?? "log-events";

builder.Services.AddSingleton(sp => new CosmosDbContext(cosmosConnection, databaseName, metricsContainer, logsContainer));
builder.Services.AddSingleton<IMetricSampleRepository, MetricSampleRepository>();
builder.Services.AddSingleton<ILogEventRepository, LogEventRepository>();
builder.Services.AddSingleton<OtlpIngestionService>();
builder.Services.AddSingleton<CostAggregationService>();
builder.Services.AddSingleton<CostTrendService>();

var app = builder.Build();

app.UseCors();
app.MapHealthEndpoints();
app.MapIngestionEndpoints();
app.MapDashboardEndpoints();

app.Run();
