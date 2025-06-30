using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Converters
{
    public class ExponentialValueConverter : NullableValueConverter
    {
        private const string _stringFormat = "0.0##E+00";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value < 0)
            {
                return NullObjectStringValue;
            }
            if ((double)value == 0.00)
            {
                return "0.00E-12";
            }

            return ((double)value).ToString(_stringFormat, CultureInfo.InvariantCulture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            value.ToString().Replace(',', '.');
            double result = 0;
            if (Double.TryParse(value.ToString(), NumberStyles.Float | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out double convValue))
                result = convValue;
            return result;
        }
    }
}
