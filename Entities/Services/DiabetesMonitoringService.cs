using Entities.Data;
using Entities.Models;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class DiabetesMonitoringService : IDiabetesMonitoringService
    {
        private readonly PharmacistRecommendationDbContext _context;

        public DiabetesMonitoringService(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task AddDiabetesMonitoringAsync(DiabetesMonitoring monitoring)
        {
            _context.DiabetesMonitorings.Add(monitoring);
            await _context.SaveChangesAsync();
        }

        public async Task<DiabetesMonitoring?> GetDiabetesMonitoringAsync(int monitoringId)
        {
            return await _context.DiabetesMonitorings.FindAsync(monitoringId);
        }

        public async Task UpdateDiabetesMonitoringAsync(DiabetesMonitoring monitoring)
        {
            _context.DiabetesMonitorings.Update(monitoring);
            await _context.SaveChangesAsync();
        }
    }

}
