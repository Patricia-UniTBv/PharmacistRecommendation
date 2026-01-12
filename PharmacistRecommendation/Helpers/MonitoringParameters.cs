using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public class MonitoringParameters
    {
        // cardio
        public int? MaxBloodPressure { get; set; }
        public int? MinBloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public int? PulseOximetry { get; set; }

        // diabetes
        public decimal? BloodGlucose { get; set; }

        // temperature
        public decimal? BodyTemperature { get; set; }
    }

}
