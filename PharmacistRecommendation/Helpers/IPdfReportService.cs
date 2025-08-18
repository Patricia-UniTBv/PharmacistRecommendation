using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public interface IPdfReportService
    {
        Task<string> CreatePatientReportAsync(int patientId,
                                              DateTime from,
                                              DateTime to);

        // New methods needed for the reports functionality
        Task<string> CreateMixedActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter);
        Task<string> CreateOwnActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter);
        Task<string> CreateConsecutivePrescriptionActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter);
        Task<string> CreateMonitoringListReportAsync(DateTime startDate, DateTime endDate, string patientFilter);
    }
}
