using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface IDiabetesMonitoringService
    {
        Task AddDiabetesMonitoringAsync(DiabetesMonitoring monitoring);
        Task<DiabetesMonitoring?> GetDiabetesMonitoringAsync(int monitoringId);
        Task UpdateDiabetesMonitoringAsync(DiabetesMonitoring monitoring);
    }

}
