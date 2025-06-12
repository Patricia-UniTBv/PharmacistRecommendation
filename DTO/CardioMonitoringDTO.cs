namespace DTO
{
    public record CardioMonitoringDTO(
        DateTime Date,
        int? MaxBloodPressure,
        int? MinBloodPressure,
        int? HeartRate,
        int? PulseOximetry,
        decimal? Height,
        decimal? Weight);
}
