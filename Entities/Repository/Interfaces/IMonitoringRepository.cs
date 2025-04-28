using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface IMonitoringRepository
    {
        Task AddMonitoringAsync(Monitoring monitoring);
        Task AddCardioMonitoringAsync(CardioMonitoring cardioMonitoring);
        Task AddTemperatureMonitoringAsync(TemperatureMonitoring temperatureMonitoring);
        Task AddDiabetesMonitoringAsync(DiabetesMonitoring diabetesMonitoring);
    }
}
