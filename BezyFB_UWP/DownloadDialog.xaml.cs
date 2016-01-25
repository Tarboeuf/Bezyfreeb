using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.EzTv;
using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    public sealed partial class DownloadDialog : ContentDialog
    {
        private Episode _episode;

        public DownloadDialog(Episode episode)
        {
            this.InitializeComponent();
            _episode = episode;
            DataContext = episode;
            if (!string.IsNullOrEmpty(episode.user[0].downloaded))
            {
                buttonTelecharger.Visibility = Visibility.Collapsed;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void Telecharger_Click(object sender, RoutedEventArgs e)
        {
            await DownloadMagnet(_episode);
        }
        private void Vu_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SousTitre_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async Task<bool> DownloadMagnet(Episode episode)
        {
            if (episode != null)
            {
                var serie = await Settings.Current.User.GetSerie(episode);
                var magnet = await Eztv.GetMagnetSerieEpisode(serie.IdEztv, episode.code);
                if (magnet != null)
                    episode.IdDownload = await Settings.Current.Freebox.Download(magnet, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""));
                else if (serie.IdEztv == null)
                {
                    Helper.AfficherMessage("Serie " + serie.ShowName + " + non configurée");
                    return false;
                }
                else
                {
                    // try to get torrent file.
                    var torrentStream = await Eztv.GetTorrentSerieEpisode(serie.IdEztv, episode.code);

                    if (null != torrentStream)
                    {
                        episode.IdDownload = await Settings.Current.Freebox.DownloadFile(torrentStream, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""), true);
                        
                        await Settings.Current.BetaSerie.SetEpisodeDownnloaded(_episode);
                    }
                    else
                    {
                        Helper.AfficherMessage("Episode " + episode.code + " de la serie " + serie.ShowName + " non trouvé");
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
