using DTO;
using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository
{
    public class CardioMonitoringRepository : ICardioMonitoringRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public CardioMonitoringRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task AddCardioMonitoringAsync(CardioMonitoring cardioMonitoring)
        {
            _context.CardioMonitorings.Add(cardioMonitoring);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CardioMonitoringDTO>> GetByPatientIdAndDateRangeAsync(int patientId, DateTime start, DateTime end)
        {
            return await _context.CardioMonitorings
                                 .Include(cm => cm.Monitoring)
                                 .Where(cm =>
                                     cm.Monitoring.PatientId == patientId &&
                                     cm.Monitoring.MonitoringDate >= start &&
                                     cm.Monitoring.MonitoringDate <= end)
                                 .Select(cm => new CardioMonitoringDTO
                                 {
                                     Date = cm.Monitoring.MonitoringDate,
                                     MaxBloodPressure = cm.MaxBloodPressure,
                                     MinBloodPressure = cm.MinBloodPressure,
                                     HeartRate = cm.HeartRate,
                                     PulseOximetry = cm.PulseOximetry,
                                     Weight = cm.Monitoring.Weight,
                                     Height = cm.Monitoring.Height
                                 })
                                 .ToListAsync();
        }

    }
}
