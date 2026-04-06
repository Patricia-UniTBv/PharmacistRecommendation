using DTO;
using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IMonitoringService
    {
        Task<List<Monitoring>> GetAllMonitoringsAsync();
        Task<Monitoring?> GetMonitoringByIdAsync(int id);
        Task<int> AddMonitoringAsync(MonitoringDTO dto, int loggedInUserId);
        Task UpdateMonitoringAsync(int monitoringId, MonitoringDTO dto);

        Task UpdateMonitoringBasicAsync(int id, string? notes, decimal? height, decimal? weight);
        Task DeleteHistoryRowAsync(int id);
        Task<IEnumerable<HistoryRowDto>> GetHistoryAsync(int patientId, DateTime from, DateTime to);
        Task<IEnumerable<HistoryRowDto>> GetHistoryByPatientIdsAsync(
    List<int> patientIds, DateTime from, DateTime to);
    }
}
