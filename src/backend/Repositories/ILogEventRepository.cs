using Backend.Models.TelemetryContracts;

namespace Backend.Repositories;

public interface ILogEventRepository
{
    Task SaveAsync(LogEvent logEvent);
}
