using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QsfpEyeDiagram.Converters
{
    public class TempToDoubleConverter : NullableValueConverter
    {
        private const string _stringFormat = "F1";

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return NullObjectStringValue;
            }

            return ((double)value).ToString(_stringFormat, CultureInfo.InvariantCulture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            //double result=0;
            //double convValue;
            //if (Double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out convValue))
            //    result = convValue;
            //return result;
        }
    }
}
