namespace DTO
{
    public class CardioMonitoringDTO
    {
        public DateTime Date { get; set; }
        public decimal? MaxBloodPressure { get; set; }
        public decimal? MinBloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? PulseOximetry { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
    }
}
