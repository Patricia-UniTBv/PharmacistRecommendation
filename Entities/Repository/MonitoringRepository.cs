using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entities.Repository
{
    public class MonitoringRepository: IMonitoringRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public MonitoringRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task AddMonitoringAsync(Monitoring monitoring)
        {
            _context.Monitorings.Add(monitoring);
            await _context.SaveChangesAsync();
        }

        public async Task AddCardioMonitoringAsync(CardioMonitoring cardioMonitoring)
        {
            _context.CardioMonitorings.Add(cardioMonitoring);
            await _context.SaveChangesAsync();
        }

        public async Task AddTemperatureMonitoringAsync(TemperatureMonitoring temperatureMonitoring)
        {
            _context.TemperatureMonitorings.Add(temperatureMonitoring);
            await _context.SaveChangesAsync();
        }

        public async Task AddDiabetesMonitoringAsync(DiabetesMonitoring diabetesMonitoring)
        {
            _context.DiabetesMonitorings.Add(diabetesMonitoring);
            await _context.SaveChangesAsync();
        }
    }
}
