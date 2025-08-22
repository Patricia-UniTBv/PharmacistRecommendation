namespace Entities.Parameters
{
    public record CardioParams(
      int? MaxBloodPressure,
      int? MinBloodPressure,
      int? HeartRate,
      int? PulseOximetry);
}
