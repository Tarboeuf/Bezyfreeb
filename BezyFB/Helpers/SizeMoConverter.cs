using System;
using System.Globalization;
using System.Windows.Data;

namespace BezyFB.Helpers
{
    public class SizeMoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long)
            {
                long ivalue = (long)value;
                return (ivalue / 1024 / 1024).ToString("## ##0") + " Mo";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}