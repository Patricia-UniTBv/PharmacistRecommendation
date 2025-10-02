using Entities.Models;

namespace Entities.Repository.Interfaces
{
    public interface IMonitoringRepository
    {
        Task<int> AddAsync(Monitoring entity);
        Task DeleteAsync(int id);
        Task<List<Monitoring>> GetByPatientAndRangeAsync(int patientId, DateTime start, DateTime end);
        Task<List<Monitoring>> GetByPatientsAndRangeAsync(List<int> patientIds, DateTime from, DateTime to);
    }
}
