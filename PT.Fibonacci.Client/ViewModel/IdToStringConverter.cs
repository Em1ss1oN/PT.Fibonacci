using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PT.Fibonacci.Client.ViewModel
{
    public class IdToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int) || !targetType.IsAssignableFrom(typeof(String)))
            {
                throw new NotSupportedException();
            }

            var intValue = (int) value;
            if (intValue <= 0)
            {
                return String.Empty;
            }

            return intValue.ToString(CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
