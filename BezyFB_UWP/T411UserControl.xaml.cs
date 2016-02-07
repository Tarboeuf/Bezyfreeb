using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.Helpers;
using BezyFB_UWP.Lib.T411;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BezyFB_UWP
{
    public sealed partial class T411UserControl : UserControl
    {
        public T411UserControl()
        {
            this.InitializeComponent();
        }

        private async void Download_OnClick(object sender, RoutedEventArgs e)
        {

            if (await Helper.ShowYesNoDialog("Êtes-vous sûr de vouloir télécharger ce film ?") == YesNo.Yes)
            {
                var torrent = DataContext as Torrent;
                if (null != torrent)
                {
                    using (var stream = Settings.Current.T411.DownloadTorrent(torrent.Id))
                    {
                        try
                        {
                            await Settings.Current.Freebox.DownloadFile(stream, torrent.Name + ".torrent", Settings.Current.PathFilm, false);
                            Helper.AfficherMessage("Le téléchargement a été rajouté");
                        }
                        catch (Exception)
                        {
                            Helper.AfficherMessage("Une erreur est survenue lors de l'ajout du téléchargement");
                        }
                    }
                }
            }
        }
    }
}
