using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QsfpEyeDiagram.Converters
{
    public class CtleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetCtleString((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetCtleString(int ctleValue)
        {
            switch (ctleValue)
            {
                case 0:
                    return $"{ctleValue} (1dB)";

                case 1:
                    return $"{ctleValue} (2dB)";

                case 2:
                    return $"{ctleValue} (3-4dB)";

                case 3:
                    return $"{ctleValue} (5dB)";

                case 4:
                    return $"{ctleValue} (6-7dB)";

                case 5:
                    return $"{ctleValue} (8dB)";

                case 6:
                    return $"{ctleValue} (9dB)";

                case 7:
                    return $"{ctleValue} (>10dB)";

                default:
                    //throw new ArgumentOutOfRangeException(nameof(ctleValue), "Показатель CTLE должен быть в диапазоне от 0 до 7.");
                    return "N/A";
            }
        }
    }
}
