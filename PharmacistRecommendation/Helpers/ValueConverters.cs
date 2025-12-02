using System.Globalization;

namespace PharmacistRecommendation.Helpers
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
       if (value is bool boolValue)
            {
                return !boolValue;
    }
            return false;
        }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
 if (value is bool boolValue)
     {
          return !boolValue;
            }
            return false;
        }
    }

    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
            if (value is string stringValue)
            {
      return !string.IsNullOrWhiteSpace(stringValue);
            }
  return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
        if (value is bool boolValue)
            {
     return boolValue;
   }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
      throw new NotImplementedException();
        }
    }
}
