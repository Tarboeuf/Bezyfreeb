using FreeboxPortableLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BezyFB.FreeboxLib
{
    /// <summary>
    /// Interaction logic for FreeboxExplorer.xaml
    /// </summary>
    public partial class FreeboxExplorer : Window
    {
        public Freebox Freebox { get { return ClientContext.Current.Freebox; } }

        public string FilePath { get; private set; }

        public FreeboxExplorer()
        {
            InitializeComponent();
            FilePath = "";
            
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

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (null != b)
            {
                FilePath += "/" + b.CommandParameter as string;
                var list = await Freebox.Ls(FilePath, true);
                fichiers.ItemsSource = list;
                labelResume.Content = FilePath;
                NbItems.Content = list.Count(s => !(s[0] == '.'));
            }
        }

        private async void Window_Initialized(object sender, System.EventArgs e)
        {
            var list = await Freebox.Ls("/", true);
            fichiers.ItemsSource = list;
            labelResume.Content = FilePath;
            NbItems.Content = list.Count(s => !(s[0] == '.'));
        }
    }
}