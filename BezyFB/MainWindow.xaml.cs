using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BezyFB.Configuration;
using BezyFB.EzTv;
using BezyFB.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BetaSerie.BetaSerie _bs;
        private readonly Utilisateur _user;
        private readonly Freebox.Freebox _freeboxApi;

        public MainWindow()
        {
            InitializeComponent();
            _bs = new BetaSerie.BetaSerie();
            tb.Text = _bs.Error;
            _user = Utilisateur.Current();
            _freeboxApi = new Freebox.Freebox();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EpisodeRoot shows = _bs.GetListeNouveauxEpisodesTest();
            if (null != shows)
                tv.ItemsSource = shows.shows;
            tb.Text = "BetaSeries : " + _bs.Error;
        }

        private void SetDl(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
            {
                _bs.SetEpisodeDownnloaded(episode);
            }
            Cursor = Cursors.Arrow;
        }

        private void MajItemsSource()
        {
            tv.ItemsSource = null;
            tv.ItemsSource = _bs.Root.shows;
        }

        private void SetSetSeen(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
            {
                _bs.SetEpisodeSeen(episode);
            }
            MajItemsSource();
            Cursor = Cursors.Arrow;
        }

        private void DlStClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            DownloadSsTitre(episode);
        }

        private void DownloadSsTitre(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var userShow = _user.GetSerie(episode.show_id);
                string pathFreebox = userShow.PathReseau;

                var str = _bs.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    var wc = new WebClient();
                    if (sousTitre != null)
                    {
                        var st = wc.DownloadData(sousTitre);
                        try
                        {
                            Stream stream = new MemoryStream(st);
                            var st2 = UnzipFromStream(stream);
                            if (st2 != null)
                                st = st2;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        string fileName = episode.show_title + "_" + episode.code + ".srt";
                        string pathreseau = pathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : "");

                        if (cbLocalNetwork.IsChecked ?? false)
                        {
                            if (!Directory.Exists(pathFreebox))
                                pathreseau = pathFreebox;
                        }

                        if (!string.IsNullOrEmpty(episode.IdDownload))
                        {
                            string file = _freeboxApi.GetFileNameDownloaded(episode.IdDownload);

                            if (!string.IsNullOrEmpty(file))
                            {
                                fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                            }
                        }

                        if ((cbLocalNetwork.IsChecked ?? false) && !Directory.Exists(pathFreebox))
                        {
                            if (string.IsNullOrEmpty(episode.IdDownload))
                            {
                                foreach (var file in Directory.GetFiles(pathreseau))
                                {
                                    if (file.Contains(episode.code))
                                    {
                                        fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                                        pathreseau = "";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Settings.Default.PathNonReseau))
                            {
                                pathreseau = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            }
                            else
                            {
                                pathreseau = Settings.Default.PathNonReseau + "/";
                            }
                            Process.Start(pathreseau);
                        }
                        File.WriteAllBytes(pathreseau + fileName, st);
                        _freeboxApi.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName);
                    }
                }
                else
                {
                    MessageBox.Show("Aucun sous titre disponible");
                }
            }
            Cursor = Cursors.Arrow;
        }

        private static byte[] UnzipFromStream(Stream zipStream)
        {
            var zipInputStream = new ZipInputStream(zipStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            if (zipEntry == null)
                return new byte[0];
            while (true)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt"))
                {
                    int file_size = (int)zipEntry.Size;
                    byte[] blob = new byte[(int)zipEntry.Size];
                    int bytes_read = 0;
                    int offset = 0;

                    while ((bytes_read = zipInputStream.Read(blob, 0, file_size)) != 0)
                    {
                        offset += bytes_read;
                    }

                    //closing every thing
                    zipInputStream.Close();
                    return blob;
                }

                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        private void GetMagnetClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            DownloadMagnet(episode);
        }

        private void DownloadMagnet(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var magnet = Eztv.GetMagnetSerieEpisode(_user.GetSerie(episode.show_id).IdEztv, episode.code);
                Console.WriteLine(magnet);

                Clipboard.SetText(magnet);

                episode.IdDownload = _freeboxApi.Download(magnet,
                    _user.GetSerie(episode.show_id).PathFreebox + "/" +
                    (_user.GetSerie(episode.show_id).ManageSeasonFolder ? episode.season : ""));
            }

            Cursor = Cursors.Arrow;
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            var c = new Configuration.Configuration(_bs, _freeboxApi);
            c.ShowDialog();
        }

        private void Download_All_Click(object sender, RoutedEventArgs e)
        {
            var root = _bs.GetListeNouveauxEpisodesTest();

            foreach (var rootShowsShow in root.shows)
            {
                foreach (var episode in rootShowsShow.unseen)
                {
                    try
                    {
                        DownloadMagnet(episode);
                        DownloadSsTitre(episode);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _freeboxApi.Deconnexion();
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            var s = ((Button)sender).CommandParameter as rootShowsShow;

            if (s != null)
            {
                if ((new WindowShow() { DataContext = _user.GetSerie(s.id, s) }).ShowDialog() ?? false)
                {
                    _user.SerializeElement();
                }
            }
        }
    }
}