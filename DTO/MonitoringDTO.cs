using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class MonitoringDTO
    {
        public int PatientId { get; set; }
        public string MonitoringType { get; set; } = null!;  
        public DateTime MonitoringDate { get; set; } = DateTime.UtcNow;

        public int? CardId { get; set; }
        public string? Notes { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

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
