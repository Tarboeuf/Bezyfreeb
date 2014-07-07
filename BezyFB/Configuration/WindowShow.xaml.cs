using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BezyFB.EzTv;
using BezyFB.Properties;

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
            Eztv ez = new Eztv();
            comboSeries.ItemsSource = ez.GetListShow();
        }

        private void ComboSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems.OfType<Eztv.Show>())
            {
                (DataContext as ShowConfiguration).IdEztv = addedItem.Id;
            }
        }
    }
}