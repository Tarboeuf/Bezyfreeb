using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BezyFB.BetaSerieLib;
using BezyFB.Properties;
using FreeboxPortableLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour FreeboxUserControl.xaml
    /// </summary>
    public partial class FreeboxUserControl : MyUserControl
    {
        public FreeboxUserControl()
        {
            InitializeComponent();
        }

        private void FreeSpace_OnClick(object sender, RoutedEventArgs e)
        {
            TailleDossierFreebox window = new TailleDossierFreebox();
            window.ShowDialog();
        }


        private async void SupprimerFilm_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = ((Button)sender).DataContext as OMDb;
            if (null != dc)
            {
                await ClientContext.Current.Freebox.DeleteFile(Settings.Default.PathFilm + dc.FileName);
            }
        }

        private async void DeleteTerminated_OnClick(object sender, RoutedEventArgs e)
        {
            var user = DataContext as UserFreebox;
            if (null != user)
            {
                foreach (var downloadItem in user.Downloads)
                {
                    if (downloadItem.Status == "done")
                    {
                        await ClientContext.Current.Freebox.DeleteTerminated(downloadItem.Id);
                    }
                }
            }
            DataContext = await ClientContext.Current.Freebox.GetInfosFreebox();
        }

        private async void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            DataContext = await ClientContext.Current.Freebox.GetInfosFreebox();
        }
    }
}
