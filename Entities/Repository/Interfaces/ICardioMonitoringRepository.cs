using DTO;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Repository.Interfaces
{
    public interface ICardioMonitoringRepository
    {
        Task<List<CardioMonitoringDTO>> GetByPatientIdAndDateRangeAsync(int patientId, DateTime start, DateTime end);
        Task AddCardioMonitoringAsync(CardioMonitoring cardioMonitoring);
    }
}
