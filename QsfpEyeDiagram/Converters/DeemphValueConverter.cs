using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QsfpEyeDiagram.Converters
{
    class DeemphValueConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetDeemphString((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetDeemphString(int deemphValue)
        {
            switch (deemphValue)
            {
                case 0:
                    return $"{deemphValue} (0dB)";

                case 1:
                    return $"{deemphValue} (0,4dB)";

                case 2:
                    return $"{deemphValue} (0,7dB)";

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    return $"{deemphValue} ({1+(deemphValue-3)*0.5}dB)";

                case 15:
                    return $"{deemphValue} (7,5dB)";

                default:
                    //throw new ArgumentOutOfRangeException(nameof(ctleValue), "Показатель CTLE должен быть в диапазоне от 0 до 7.");
                    return "N/A";
            }
        }

    }
}
