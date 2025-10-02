using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class MonitoringRepository: IMonitoringRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public MonitoringRepository(PharmacistRecommendationDbContext context) => _context = context;

        public async Task<int> AddAsync(Monitoring entity)
        {
            _context.Monitorings.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Monitorings.FindAsync(id);
            if (entity == null)
                throw new Exception("Monitorizare inexistentă");

            _context.Monitorings.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Monitoring>> GetByPatientAndRangeAsync(
         int patientId, DateTime from, DateTime to)
        {
            return await _context.Monitorings
                     .AsNoTracking()
                     .Where(m => m.PatientId == patientId &&
                                 m.MonitoringDate >= from &&
                                 m.MonitoringDate < to.AddDays(1))
                     .OrderBy(m => m.MonitoringDate)
                     .ToListAsync();

        }

        public async Task<List<Monitoring>> GetByPatientsAndRangeAsync(List<int> patientIds, DateTime from, DateTime to)
        {
            return await _context.Monitorings
                .AsNoTracking()
                .Where(m => patientIds.Contains(m.PatientId) &&
                            m.MonitoringDate >= from &&
                            m.MonitoringDate < to.AddDays(1))
                .OrderBy(m => m.MonitoringDate)
                .ToListAsync();
        }

    }
}
