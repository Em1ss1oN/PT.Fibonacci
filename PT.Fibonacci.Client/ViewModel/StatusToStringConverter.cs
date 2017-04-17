using System;
using System.Globalization;
using System.Windows.Data;
using PT.Fibonacci.Client.Model;

namespace PT.Fibonacci.Client.ViewModel
{
    public class StatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CalculatorStatus) || !targetType.IsAssignableFrom(typeof(String)))
            {
                throw new NotSupportedException();
            }

            var status = (CalculatorStatus) value;
            switch (status)
            {
                case CalculatorStatus.NotStarted:
                    return @"Start";
                case CalculatorStatus.Started:
                    return @"Stop";
                case CalculatorStatus.Starting:
                    return @"Starting...";
                case CalculatorStatus.Stopping:
                    return @"Stopping...";
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}