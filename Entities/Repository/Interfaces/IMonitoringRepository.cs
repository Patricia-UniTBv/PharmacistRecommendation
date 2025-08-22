using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IMonitoringRepository
    {
        Task<int> AddAsync(Monitoring entity);
        Task<List<Monitoring>> GetByPatientAndRangeAsync(int patientId, DateTime start, DateTime end);
    }
}
