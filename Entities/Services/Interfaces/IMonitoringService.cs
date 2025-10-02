using DTO;
using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IMonitoringService
    {
        Task<int> AddMonitoringAsync(MonitoringDTO dto, int loggedInUserId);

        Task DeleteHistoryRowAsync(int id);
        Task<IEnumerable<HistoryRowDto>> GetHistoryAsync(int patientId, DateTime from, DateTime to);
        Task<IEnumerable<HistoryRowDto>> GetHistoryByPatientIdsAsync(
    List<int> patientIds, DateTime from, DateTime to);
    }
}
