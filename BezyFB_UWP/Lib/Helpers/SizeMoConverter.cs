using System;
using Windows.UI.Xaml.Data;

namespace BezyFB_UWP.Lib.Helpers
{
    public class SizeMoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long)
            {
                long ivalue = (long)value;
                return (ivalue / 1024 / 1024).ToString("## ##0") + " Mo";
            }
            return null;
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}