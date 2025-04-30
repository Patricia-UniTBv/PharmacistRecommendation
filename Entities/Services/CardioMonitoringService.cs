using DTO;
using Entities.Data;
using Entities.Models;
using Entities.Repository;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class CardioMonitoringService : ICardioMonitoringService
    {
        private readonly ICardioMonitoringRepository _repository;

        public CardioMonitoringService(ICardioMonitoringRepository repository)
        {
            _repository = repository;
        }

        //public async Task AddCardioMonitoringAsync(CardioMonitoring monitoring)
        //{
        //    try
        //    {
        //        _context.CardioMonitorings.Add(monitoring);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }

        //}

        //public async Task<CardioMonitoring?> GetCardioMonitoringAsync(int monitoringId)
        //{
        //    return await _context.CardioMonitorings.FindAsync(monitoringId);
        //}

        //public async Task UpdateCardioMonitoringAsync(CardioMonitoring monitoring)
        //{
        //    _context.CardioMonitorings.Update(monitoring);
        //    await _context.SaveChangesAsync();
        //}

        public async Task SaveCardioMonitoringAsync(CardioMonitoring cardio)
        {
            try
            {
                if (cardio != null)
                {
                    await _repository.AddCardioMonitoringAsync(cardio);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la adăugarea monitorizării cardio: {ex.Message}");
                throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea cardio.", ex);
            }
        }

        public async Task<List<CardioMonitoringDTO>> GetHistoryAsync(int patientId, DateTime start, DateTime end)
        {
            try
            {
                return await _repository.GetByPatientIdAndDateRangeAsync(patientId, start, end);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("A apărut o eroare la obținerea istoricului monitorizărilor cardio.", ex);
            }
        }
    }

}
