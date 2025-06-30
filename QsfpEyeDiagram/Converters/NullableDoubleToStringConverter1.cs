using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Converters
{
    public class NullableDoubleToStringConverter1 : NullableValueConverter
    {
        private const string _stringFormat = "F2";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return NullObjectStringValue;
            }
            if ((double)value == default && parameter != null)
                return NullObjectStringValue;

            return ((double)value).ToString(_stringFormat,CultureInfo.InvariantCulture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            value.ToString().Replace(',', '.');
            double result = 0;
            if (Double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double convValue))
                result = convValue;
            return result;
        }
    }
}
