using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BezyFB.Freebox
{
    /// <summary>
    /// Interaction logic for FreeboxExplorer.xaml
    /// </summary>
    public partial class FreeboxExplorer : Window
    {
        private Freebox _Freebox;

        public Freebox Freebox { get { return _Freebox; } }

        public string FilePath { get; private set; }

        public FreeboxExplorer()
        {
            InitializeComponent();
            FilePath = "";
            _Freebox = new Freebox();

            var list = _Freebox.Ls("/", true);
            fichiers.ItemsSource = list;
            labelResume.Content = FilePath;
            NbItems.Content = list.Count(s => !(s[0] == '.'));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (fichiers.SelectedItem != null)
            {
                FilePath += "/" + fichiers.SelectedItem as string;
                DialogResult = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (null != b)
            {
                FilePath += "/" + b.CommandParameter as string;
                var list = _Freebox.Ls(FilePath, true);
                fichiers.ItemsSource = list;
                labelResume.Content = FilePath;
                NbItems.Content = list.Count(s => !(s[0] == '.'));
            }
        }
    }
}