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
    public class CardioMonitoringService : ICardioMonitoringService
    {
        private readonly PharmacistRecommendationDbContext _context;

        public CardioMonitoringService(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task AddCardioMonitoringAsync(CardioMonitoring monitoring)
        {
            try
            {
                _context.CardioMonitorings.Add(monitoring);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public async Task<CardioMonitoring?> GetCardioMonitoringAsync(int monitoringId)
        {
            return await _context.CardioMonitorings.FindAsync(monitoringId);
        }

        public async Task UpdateCardioMonitoringAsync(CardioMonitoring monitoring)
        {
            _context.CardioMonitorings.Update(monitoring);
            await _context.SaveChangesAsync();
        }
    }

}
