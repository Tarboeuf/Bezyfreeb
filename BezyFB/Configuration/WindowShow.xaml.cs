using BezyFB.EzTv;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BezyFB.Configuration
{
    /// <summary>
    /// Logique d'interaction pour WindowShow.xaml
    /// </summary>
    public partial class WindowShow : Window
    {
        public WindowShow()
        {
            InitializeComponent();
            DataContextChanged += WindowShow_DataContextChanged;
        }

        private void WindowShow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ShowConfig.IdEztv))
            {
                var ez = new Eztv();
                var liste = ez.GetListShow().ToList();
                comboSeries.ItemsSource = liste;
                ShowConfig.IdEztv = liste.Where(c => ShowConfig.ShowName.ToLower() == c.Name.ToLower()).Select(c => c.Name).FirstOrDefault();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LoadSerieEZTV(object sender, RoutedEventArgs e)
        {
            var ez = new Eztv();
            comboSeries.ItemsSource = ez.GetListShow();
        }

        private void ComboSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems.OfType<Eztv.Show>())
            {
                ShowConfig.IdEztv = addedItem.Id;
            }
        }

        private ShowConfiguration ShowConfig
        {
            get { return DataContext as ShowConfiguration; }
        }
    }
}