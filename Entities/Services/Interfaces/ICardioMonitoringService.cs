using DTO;
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
        Task SaveCardioMonitoringAsync(CardioMonitoring cardio);
        Task<List<CardioMonitoringDTO>> GetHistoryAsync(int patientId, DateTime start, DateTime end);
    }
}
