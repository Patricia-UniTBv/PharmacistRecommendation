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
    public class TemperatureMonitoringService : ITemperatureMonitoringService
    {
        private readonly PharmacistRecommendationDbContext _context;

        public TemperatureMonitoringService(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task AddTemperatureMonitoringAsync(TemperatureMonitoring monitoring)
        {
            _context.TemperatureMonitorings.Add(monitoring);
            await _context.SaveChangesAsync();
        }

        public async Task<TemperatureMonitoring?> GetTemperatureMonitoringAsync(int monitoringId)
        {
            return await _context.TemperatureMonitorings.FindAsync(monitoringId);
        }

        public async Task UpdateTemperatureMonitoringAsync(TemperatureMonitoring monitoring)
        {
            _context.TemperatureMonitorings.Update(monitoring);
            await _context.SaveChangesAsync();
        }
    }

}
