using DTO;
using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IMonitoringService
    {
        Task<int> AddMonitoringAsync(MonitoringDTO dto, int loggedInUserId);
        Task<IEnumerable<HistoryRowDto>> GetHistoryAsync(int patientId, DateTime from, DateTime to);
    }
}
