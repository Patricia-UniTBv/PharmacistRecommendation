using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public class UpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string s ? char.ToUpper(s[0]) + s[1..] : value;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => v;
    }

}
