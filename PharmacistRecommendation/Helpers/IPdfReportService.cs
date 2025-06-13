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
    }

}
