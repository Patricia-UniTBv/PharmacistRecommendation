using Entities.Models;
using System.Globalization;
using System.Text.Json;

namespace PharmacistRecommendation.Converters
{
    public class MonitoringTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string json || string.IsNullOrWhiteSpace(json)) return "—";
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (dict == null) return "—";
                if (dict.ContainsKey("MaxBloodPressure") || dict.ContainsKey("HeartRate")) return "cardio";
                if (dict.ContainsKey("BloodGlucose"))                                      return "diabetes";
                if (dict.ContainsKey("BodyTemperature"))                                   return "temperature";
            }
            catch { }
            return "—";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
