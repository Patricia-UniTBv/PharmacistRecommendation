using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface ICardioMonitoringService
    {
        Task AddCardioMonitoringAsync(CardioMonitoring monitoring);
        Task<CardioMonitoring?> GetCardioMonitoringAsync(int monitoringId);
        Task UpdateCardioMonitoringAsync(CardioMonitoring monitoring);
    }
}
