using System.Windows;
using System.Windows.Controls;

namespace BezyFB
{
    public class MyUserControl: UserControl
    {
        public void SetStatusText(string status)
        {
            Window.SetStatusText(status);
        }

        public MainWindow Window { get; } = ((MainWindow)Application.Current.MainWindow);
    }
}
