using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface ITemperatureMonitoringService
    {
        Task AddTemperatureMonitoringAsync(TemperatureMonitoring monitoring);
        Task<TemperatureMonitoring?> GetTemperatureMonitoringAsync(int monitoringId);
        Task UpdateTemperatureMonitoringAsync(TemperatureMonitoring monitoring);
    }
}
