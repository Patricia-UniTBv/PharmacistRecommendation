using DTO;
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
        Task<int> AddMonitoringAsync(MonitoringDTO dto, int loggedInUserId);
        Task<IEnumerable<HistoryRowDto>> GetHistoryAsync(int patientId, DateTime from, DateTime to);
        //Task<IEnumerable<CardioMonitoringDTO>> GetCardioAsync(int patientId, DateTime start, DateTime end);
        //Task<IEnumerable<DiabetesMonitoringDTO>> GetDiabetesAsync(int patientId, DateTime start, DateTime end);
        //Task<IEnumerable<TemperatureMonitoringDTO>> GetTemperatureAsync(int patientId, DateTime start, DateTime end);
    }
}
