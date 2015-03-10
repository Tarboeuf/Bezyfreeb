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
            tb.Items.Clear();
            tb.Items.Add(_bs.Error);
            _user = Utilisateur.Current();
            _freeboxApi = new Freebox.Freebox();

            Button_Click(this, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EpisodeRoot shows = _bs.GetListeNouveauxEpisodesTest();
            if (null != shows)
                tv.ItemsSource = shows.shows;
            tb.Items.Clear();
            tb.Items.Add("BetaSeries: " + _bs.Error);
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
            //MajItemsSource();
            Cursor = Cursors.Arrow;
        }

        private void DlTout(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            DownloadMagnet(episode);
            DownloadSsTitre(episode);
            _bs.SetEpisodeDownnloaded(episode);
            Cursor = Cursors.Arrow;
        }

        private void DlStClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            DownloadSsTitre(episode);
        }

        private string ExtractEncoding(string movieFilePath)
        {
            if (movieFilePath.Contains("LOL"))
                return "LOL";
            if (movieFilePath.Contains("2HD"))
                return "2HD";
            return "";
        }

        private void DownloadSsTitre(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var userShow = _user.GetSerie(episode);
                string pathFreebox = userShow.PathReseau;

                var str = _bs.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    string fileName = episode.show_title + "_" + episode.code + ".srt";

                    string encoding = "";
                    if (!string.IsNullOrEmpty(episode.IdDownload))
                    {
                        string file = _freeboxApi.GetFileNameDownloaded(episode.IdDownload);

                        if (!string.IsNullOrEmpty(file))
                        {
                            if (file.LastIndexOf('.') <= (file.Length - 5))
                                fileName = file + ".srt";
                            else
                                fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                        }
                    }
                    string pathreseau = pathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : "");

                    if ((cbLocalNetwork.IsChecked ?? false) && !Directory.Exists(pathFreebox))
                    {
                        if (string.IsNullOrEmpty(episode.IdDownload))
                        {
                            foreach (var file in Directory.GetFiles(pathreseau))
                            {
                                if (file.Contains(episode.code))
                                {
                                    if (file.LastIndexOf('.') <= (file.Length - 5))
                                        fileName = file + ".srt";
                                    else
                                        fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                                    pathreseau = "";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(episode.IdDownload))
                        {
                            var lst = _freeboxApi.Ls(Settings.Default.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), false);
                            string f = lst.FirstOrDefault(s => s.Contains(episode.code) && !s.EndsWith(".srt"));
                            if (null != f)
                            {
                                fileName = f.Replace(f.Substring(f.LastIndexOf('.')), ".srt");
                            }
                        }
                        if (string.IsNullOrEmpty(Settings.Default.PathNonReseau))
                        {
                            pathreseau = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        }
                        else
                        {
                            pathreseau = Settings.Default.PathNonReseau + "/";
                        }

                        //Process.Start(pathreseau);
                    }
                    encoding = ExtractEncoding(fileName);
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    var wc = new WebClient();
                    if (sousTitre != null)
                    {
                        var st = wc.DownloadData(sousTitre);
                        Clipboard.SetText(sousTitre);
                        try
                        {
                            Stream stream = new MemoryStream(st);
                            var st2 = UnzipFromStream(stream, encoding);
                            if (st2 != null)
                            {
                                st = Encoding.Convert(Encoding.Default, Encoding.UTF8, st2);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Settings.Default.AffichageErreurMessageBox)
                                MessageBox.Show(ex.Message);
                            else
                                Console.WriteLine(ex.Message);
                        }

                        if (cbLocalNetwork.IsChecked ?? false)
                        {
                            if (!Directory.Exists(pathFreebox))
                                pathreseau = pathFreebox;
                        }

                        File.WriteAllBytes(pathreseau + fileName, st);
                        _freeboxApi.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName);
                        _freeboxApi.CleanUpload();
                        File.Delete(pathreseau + fileName);

                        tb.Items.Clear();
                        tb.Items.Add("Fichier : " + fileName);
                    }
                }
                else
                {
                    if (Settings.Default.AffichageErreurMessageBox)
                        MessageBox.Show("Aucun sous titre disponible");
                    else
                        Console.WriteLine("Aucun sous titre disponible");
                }
            }
            Cursor = Cursors.Arrow;
        }

        private static byte[] UnzipFromStream(Stream zipStream, string encoding)
        {
            var zipInputStream = new ZipInputStream(zipStream);
            
            if (!zipInputStream.CanRead)
                return null;
            ZipEntry zipEntry = null;
            try
            {
                zipEntry = zipInputStream.GetNextEntry();
            }
            catch (Exception)
            {
                return null;
            }
            if (zipEntry == null)
                return new byte[0];
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt") && entryFileName.Contains(encoding))
                {
                    Clipboard.SetText(entryFileName);
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
            zipInputStream.Seek(0, SeekOrigin.Begin);
            zipEntry = zipInputStream.GetNextEntry();
            if (zipEntry == null)
                return new byte[0];
            while (true)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt"))
                {
                    Clipboard.SetText(entryFileName);
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
                var magnet = Eztv.GetMagnetSerieEpisode(_user.GetSerie(episode).IdEztv, episode.code);
                if (magnet != null)
                    episode.IdDownload = _freeboxApi.Download(magnet, _user.GetSerie(episode).PathFreebox + "/" +
                                                                      (_user.GetSerie(episode).ManageSeasonFolder ? episode.season : ""));
                else if (_user.GetSerie(episode).IdEztv == null)
                {
                    if (Settings.Default.AffichageErreurMessageBox)
                        MessageBox.Show("Serie non configurée");
                    else
                        Console.WriteLine("Serie non configurée");
                }
                else
                {
                    if (Settings.Default.AffichageErreurMessageBox)
                        MessageBox.Show("Serie non trouvée");
                    else
                        Console.WriteLine("Serie non trouvée");
                }
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
            var errors = "";

            foreach (var rootShowsShow in root.shows)
            {
                foreach (var episode in rootShowsShow.unseen)
                {
                    try
                    {
                        DownloadMagnet(episode);
                    }
                    catch (Exception ex)
                    {
                        if (Settings.Default.AffichageErreurMessageBox)
                            MessageBox.Show(ex.Message);
                        else
                            Console.WriteLine(ex.Message);
                        errors += episode.show_title + "(" + episode.code + ") : " +  ex.Message + "\r\n";
                    }

                    try
                    {
                        DownloadSsTitre(episode);
                    }
                    catch (Exception ex)
                    {
                        if (Settings.Default.AffichageErreurMessageBox)
                            MessageBox.Show(ex.Message);
                        else
                            Console.WriteLine(ex.Message);
                        errors += episode.show_title + "(" + episode.code + ") : " + ex.Message + "\r\n";
                    }
                }
            }

            if (!String.IsNullOrEmpty(errors))
                MessageBox.Show(errors);
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
                if ((new WindowShow { DataContext = _user.GetSerie(s) }).ShowDialog() ?? false)
                {
                    _user.SerializeElement();
                }
            }
        }
    }
}