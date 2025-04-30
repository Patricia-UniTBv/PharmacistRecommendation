using DTO;
using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entities.Services
{
    public class MonitoringService: IMonitoringService
    {
        private readonly IMonitoringRepository _monitoringRepository;
        public MonitoringService(IMonitoringRepository monitoringRepository)
        {
            _monitoringRepository = monitoringRepository;
        }

        public async Task SaveMonitoringAsync(Monitoring monitoring)
        {
            try
            {
                await _monitoringRepository.AddMonitoringAsync(monitoring);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eroare la adăugarea monitorizării: {ex.Message}");
                throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea.", ex);
            }
        }

        //public async Task SaveCardioMonitoringAsync(CardioMonitoring cardio)
        //{
        //    try
        //    {
        //        if (cardio != null)
        //        {
        //            await _monitoringRepository.AddCardioMonitoringAsync(cardio);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării cardio: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea cardio.", ex);
        //    }
        //}

        //public async Task SaveTemperatureMonitoringAsync(TemperatureMonitoring temperature)
        //{
        //    try
        //    {
        //        if (temperature != null)
        //        {
        //            await _monitoringRepository.AddTemperatureMonitoringAsync(temperature);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării de temperatură: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea de temperatură.", ex);
        //    }
        //}

        //public async Task SaveDiabetesMonitoringAsync(DiabetesMonitoring diabetes)
        //{
        //    try
        //    {
        //        if (diabetes != null)
        //        {
        //            await _monitoringRepository.AddDiabetesMonitoringAsync(diabetes);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării de diabet: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea de diabet.", ex);
        //    }
        //}

    }
}
