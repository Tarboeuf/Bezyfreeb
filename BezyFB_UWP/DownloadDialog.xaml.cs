﻿using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.EzTv;
using BezyFB_UWP.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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
            ProgressBarDC.Current.IsProgress = true;
            await DownloadMagnet(_episode);
            await DownloadSsTitre(_episode);
            ProgressBarDC.Current.IsProgress = false;
        }
        private async void Vu_Click(object sender, RoutedEventArgs e)
        {
            await Settings.Current.BetaSerie.SetEpisodeSeen(_episode);
            Hide();
        }
        private async void SousTitre_Click(object sender, RoutedEventArgs e)
        {
            ProgressBarDC.Current.IsProgress = true;
            await DownloadSsTitre(_episode);
            ProgressBarDC.Current.IsProgress = false;
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
                if (serie.IdEztv == null)
                {
                    Helper.AfficherMessage("La série n'est pas configuré");
                    return false;
                }

                var magnet = await Eztv.GetMagnetSerieEpisode(serie.IdEztv, episode.code);
                if (magnet != null)
                    episode.IdDownload = await Settings.Current.Freebox.Download(magnet, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""));
                else
                {
                    // try to get torrent file.
                    var torrentStream = await Eztv.GetTorrentSerieEpisode(serie.IdEztv, episode.code);

                    if (null != torrentStream)
                    {
                        episode.IdDownload = await Settings.Current.Freebox.DownloadFile(torrentStream, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""), true);

                    }
                    else
                    {
                        Helper.AfficherMessage("Episode " + episode.code + " de la serie " + serie.ShowName + " non trouvé");
                        return false;
                    }
                }
            }

            await Settings.Current.BetaSerie.SetEpisodeDownnloaded(_episode);
            Hide();
            return true;
        }
        private async Task<bool> DownloadSsTitre(Episode episode)
        {
            if (episode != null)
            {
                var userShow = await Settings.Current.User.GetSerie(episode);
                string pathFreebox = ApplicationData.Current.LocalFolder.Path;

                var str = await Settings.Current.BetaSerie.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    string fileName = episode.show_title + "_" + episode.code + ".srt";

                    if (!string.IsNullOrEmpty(episode.IdDownload))
                    {
                        string file = await Settings.Current.Freebox.GetFileNameDownloaded(episode.IdDownload);

                        if (!string.IsNullOrEmpty(file))
                        {
                            if (file.LastIndexOf('.') <= (file.Length - 5))
                                fileName = file + ".srt";
                            else
                                fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                        }
                    }
                    string pathreseau = pathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : "");

                    if (string.IsNullOrEmpty(episode.IdDownload))
                    {
                        var lst = await Settings.Current.Freebox.Ls(Settings.Current.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), false);
                        if (lst != null)
                        {
                            string f = lst.FirstOrDefault(s => s.Contains(episode.code) && !s.EndsWith(".srt"));
                            if (null != f)
                            {
                                fileName = f.Replace(f.Substring(f.LastIndexOf('.')), ".srt");
                            }
                        }
                    }

                    //Process.Start(pathreseau);

                    var encoding = ExtractEncoding(fileName);
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    var wc = new HttpClient();
                    if (sousTitre != null)
                    {
                        var st = await wc.GetInputStreamAsync(new Uri(sousTitre));
                        string st2;
                        try
                        {
                            st2 = UnzipFromStream(st, encoding);
                            if (st2 == null)
                            {
                                st = await wc.GetInputStreamAsync(new Uri(sousTitre));
                                using (StreamReader sr = new StreamReader(st.AsStreamForRead()))
                                {
                                    st2 = sr.ReadToEnd();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.AfficherMessage(ex.Message);
                            return false;
                        }

                        File.WriteAllText(pathreseau + fileName, st2);
                        await Settings.Current.Freebox.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName);
                        await Settings.Current.Freebox.CleanUpload();
                        File.Delete(pathreseau + fileName);

                    }
                }
                else
                {
                    Helper.AfficherMessage("Aucun sous titre disponible");
                    return false;
                }
            }
            return true;
        }
        private static string UnzipFromStream(IInputStream st, string encoding)
        {
            try
            {
                var archive = new ZipArchive(st.AsStreamForRead());
                //var zipInputStream = new ZipInputStream(zipStream);

                foreach (var zipEntry in archive.Entries)
                {
                    string entryFileName = zipEntry.Name;

                    if (entryFileName.Contains(".srt") && entryFileName.Contains(encoding))
                    {
                        using (var stream = zipEntry.Open())
                        using (var sr = new StreamReader(stream, Encoding.UTF7, true))
                        {
                            var result = sr.ReadToEnd();
                            return result;
                        }
                    }
                }
                foreach (var zipEntry in archive.Entries)
                {
                    string entryFileName = zipEntry.Name;

                    if (entryFileName.Contains(".srt"))
                    {
                        using (var stream = zipEntry.Open())
                        using (var sr = new StreamReader(stream))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        private string ExtractEncoding(string movieFilePath)
        {
            if (movieFilePath.Contains("LOL"))
                return "LOL";
            if (movieFilePath.Contains("2HD"))
                return "2HD";
            return "";
        }

    }
}
