using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IMonitoringService
    {
        Task SaveMonitoringAsync(Monitoring monitoring);
        Task SaveCardioMonitoringAsync(CardioMonitoring cardio);
        Task SaveTemperatureMonitoringAsync(TemperatureMonitoring temperature);
        Task SaveDiabetesMonitoringAsync(DiabetesMonitoring diabetes);
    }
}
