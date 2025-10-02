using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class HistoryRowDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? MaxBloodPressure { get; set; }
        public int? MinBloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public int? PulseOximetry { get; set; }
        public decimal? BloodGlucose { get; set; }
        public decimal? BodyTemperature { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
    }

}
