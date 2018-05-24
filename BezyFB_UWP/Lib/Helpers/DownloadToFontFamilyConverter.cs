using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace BezyFB_UWP.Lib.Helpers
{
    public class DownloadToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var str = value as string;
            if(!string.IsNullOrEmpty(str))
            {
                return FontStyle.Italic;
            }
            return FontStyle.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
