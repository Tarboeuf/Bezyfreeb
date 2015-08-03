using System.ComponentModel;
using System.Web;
using BezyFB.BetaSerie;
using BezyFB.Configuration;
using BezyFB.EzTv;
using BezyFB.Freebox;
using BezyFB.Properties;
using BezyFB.T411;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
using Microsoft.Win32;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BetaSerie.BetaSerie _bs;
        private Utilisateur _user;
        private Freebox.Freebox _freeboxApi;
        private T411Client _client;

        public MainWindow()
        {
            InitializeComponent();
            SetStatusText("Veuillez choisir votre catégorie");

            T411Client.BaseAddress = "https://api.t411.io/";
        }

        public void InitialiseElements(bool forceFreebox, bool forceBetaSerie, bool forceT411, bool forceUser)
        {
            if(forceFreebox || _freeboxApi == null)
                _freeboxApi = new Freebox.Freebox();

            if (forceBetaSerie || _bs == null)
                _bs = new BetaSerie.BetaSerie();

            if (forceT411 || _client == null)
                _client = new T411Client(Settings.Default.LoginT411, Settings.Default.PassT411);

            if (forceUser || _user == null)
                _user = Utilisateur.Current();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadBetaseries();
        }

        private void SetDl(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;
            
            if (episode != null)
                _bs.SetEpisodeDownnloaded(episode);
            Cursor = Cursors.Arrow;
        }

        private void SetSetSeen(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
                _bs.SetEpisodeSeen(episode);
            Cursor = Cursors.Arrow;
        }

        private void DlTout(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (DownloadMagnet(episode) && DownloadSsTitre(episode))
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

        private bool DownloadSsTitre(Episode episode)
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

                    if (string.IsNullOrEmpty(episode.IdDownload))
                    {
                        var lst = _freeboxApi.Ls(Settings.Default.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), false);
                        if (lst != null)
                        {
                            string f = lst.FirstOrDefault(s => s.Contains(episode.code) && !s.EndsWith(".srt"));
                            if (null != f)
                            {
                                fileName = f.Replace(f.Substring(f.LastIndexOf('.')), ".srt");
                            }
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

                    var encoding = ExtractEncoding(fileName);
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    var wc = new WebClient();
                    if (sousTitre != null)
                    {
                        var st = wc.DownloadData(sousTitre);
                        Clipboard.SetText(sousTitre);
                        try
                        {
                            var st2 = UnzipFromStream(st, encoding);
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
                            Cursor = Cursors.Arrow;
                            return false;
                        }

                        File.WriteAllBytes(pathreseau + fileName, st);
                        _freeboxApi.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName);
                        _freeboxApi.CleanUpload();
                        File.Delete(pathreseau + fileName);

                        SetStatusText("Fichier : " + fileName);
                    }
                }
                else
                {
                    if (Settings.Default.AffichageErreurMessageBox)
                        MessageBox.Show("Aucun sous titre disponible");
                    else
                        Console.WriteLine("Aucun sous titre disponible");
                    Cursor = Cursors.Arrow;
                    return false;
                }
            }
            Cursor = Cursors.Arrow;
            return true;
        }

        private void SetStatusText(string text)
        {
            StatusBar.Items.Clear();
            StatusBar.Items.Add(text);
        }

        private static byte[] UnzipFromStream(byte[] st, string encoding)
        {
            MemoryStream zipStream = new MemoryStream(st);
            var zipInputStream = new ZipInputStream(zipStream);

            if (!zipInputStream.CanRead)
                return null;
            ZipEntry zipEntry;
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
                    int fileSize = (int)zipEntry.Size;
                    byte[] blob = new byte[(int)zipEntry.Size];

                    while ((zipInputStream.Read(blob, 0, fileSize)) != 0)
                    {
                    }

                    //closing every thing
                    zipInputStream.Close();
                    return blob;
                }

                zipEntry = zipInputStream.GetNextEntry();
            }
            if (zipInputStream.CanSeek)
            {
                zipInputStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                zipStream = new MemoryStream(st);
                zipInputStream = new ZipInputStream(zipStream);
            }
            zipEntry = zipInputStream.GetNextEntry();
            if (zipEntry == null)
                return new byte[0];
            while (true)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt"))
                {
                    Clipboard.SetText(entryFileName);
                    int fileSize = (int)zipEntry.Size;
                    byte[] blob = new byte[(int)zipEntry.Size];

                    while ((zipInputStream.Read(blob, 0, fileSize)) != 0)
                    {
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

        private bool DownloadMagnet(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var serie = _user.GetSerie(episode);
                var magnet = Eztv.GetMagnetSerieEpisode(serie.IdEztv, episode.code);
                if (magnet != null)
                    episode.IdDownload = _freeboxApi.Download(magnet, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""));
                else if (serie.IdEztv == null)
                {
                    if (Settings.Default.AffichageErreurMessageBox)
                        MessageBox.Show("Serie non configurée");
                    else
                        Console.WriteLine("Serie non configurée");

                    Cursor = Cursors.Arrow;
                    return false;
                }
                else
                {
                    // try to get torrent file.
                    var torrentStream = Eztv.GetTorrentSerieEpisode(serie.IdEztv, episode.code);

                    if (null != torrentStream)
                    {
                        //ByteArrayToFile("E:\\test.torrent", torrentStream);
                        episode.IdDownload = _freeboxApi.DownloadFile(torrentStream, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""), true);
                    }
                    else
                    {
                        if (Settings.Default.AffichageErreurMessageBox)
                            MessageBox.Show("Episode " + episode.code + " non trouvé");
                        else
                            Console.WriteLine("Episode " + episode.code + " non trouvé");
                        Cursor = Cursors.Arrow;
                        return false;
                    }
                }
            }

            Cursor = Cursors.Arrow;
            return true;
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            InitialiseElements(false, false, false, false);
            var c = new Configuration.Configuration(_bs, _freeboxApi);
            c.ShowDialog();
            InitialiseElements(true, true, true, true);
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
                        errors += episode.show_title + "(" + episode.code + ") : " + ex.Message + "\r\n";
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

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitialiseElements(false, false, false, false);
            if (tc.SelectedIndex == 1 && lv.ItemsSource == null)
            {
                LoadT411();
            }
            if (tc.SelectedIndex == 2)
            {
                if (!(TabFreebox.DataContext is UserFreebox))
                {
                    var uf = _freeboxApi.GetInfosFreebox();
                    TabFreebox.DataContext = uf;

                    uf.LoadMovies(Dispatcher);
                }
            }
        }

        private void ButtonTelechargerTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    using (var stream = _client.DownloadTorrent(torrent.Torrent.Id))
                    {
                        if (string.IsNullOrEmpty(Settings.Default.TokenFreebox))
                        {
                            var sfd = new SaveFileDialog();
                            sfd.FileName = torrent.Name + ".torrent";
                            if (sfd.ShowDialog() ?? false)
                            {
                                using (var s = File.Create(sfd.FileName))
                                {
                                    stream.CopyTo(s);
                                }
                            }
                        }
                        else
                        {
                            _freeboxApi.DownloadFile(stream, torrent.Name + ".torrent", Settings.Default.PathFilm, false);
                        }
                    }
                }
            }
        }

        private void ButtonExtraInfoOnClick(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    torrent.Initialiser();
                }
            }
        }

        private void SupprimerFilm_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = ((Button)sender).DataContext as OMDb;
            if (null != dc)
            {
                _freeboxApi.DeleteFile(Settings.Default.PathFilm + dc.FileName);
            }
        }

        private void buttonT411Rechercher_Click(object sender, RoutedEventArgs e)
        {
            StatusBar.Items.Clear();
            StatusBar.Items.Add("Connecté t411");
            if (string.IsNullOrEmpty(textBoxRechercheT411.Text))
            {
                lv.ItemsSource = _client.GetTopWeek().Where(t => t.CategoryName == ((SousCategorie)comboCategoryT411.SelectedValue).Cat.Name).OrderByDescending(t => t.Times_completed).Select(t => new MyTorrent(t));
            }
            else
            {
                lv.ItemsSource = _client.GetQuery(string.Format("{0}", HttpUtility.UrlEncode(textBoxRechercheT411.Text)), 
                    new QueryOptions
                    {
                        CategoryIds = new List<int>
                        {
                            ((SousCategorie)comboCategoryT411.SelectedValue).Cat.Id
                        },
                        Limit = 1000
                    }).Torrents
                    .OrderByDescending(t => t.Times_completed).Select(t => new MyTorrent(t));
            }
            StatusBar.Items.Add("torrents récupéré");
        }

        private void Quitter_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadBetaseries()
        {
            pb.Visibility = Visibility.Visible;
            SetStatusText(_bs.Error);
            
            var t = new Task<EpisodeRoot>(() => _bs.GetListeNouveauxEpisodesTest());
            t.ContinueWith(r => Dispatcher.BeginInvoke(new Action(() =>
            {
                if (r != null && r.Result != null)
                    tv.ItemsSource = r.Result.shows;
                gridButton.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Collapsed;
            })));
            t.Start();
        }

        private void LoadT411()
        {
            SetStatusText("Chargement des données T411");
            var worker = new BackgroundWorker();
            pb.Visibility = Visibility.Visible;
            worker.DoWork += delegate(object sender, DoWorkEventArgs args)
            {
                T411Client.BaseAddress = "https://api.t411.io/";
                if (_client == null)
                    _client = new T411Client(Settings.Default.LoginT411, Settings.Default.PassT411);

                Dispatcher.BeginInvoke((Action)(() => lv.ItemsSource =
                    _client.GetTopWeek()
                        .Where(t => t.CategoryName == "Film")
                        .OrderByDescending(t => t.Times_completed)
                        .Select(t => new MyTorrent(t))));


                var categories = new List<SousCategorie>();
                foreach (var category1 in _client.GetCategory())
                {
                    foreach (var cat in category1.Value.Cats)
                    {
                        categories.Add(new SousCategorie(cat.Value, category1.Value.Name));
                    }
                }
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    comboCategoryT411.ItemsSource = categories;
                    comboCategoryT411.SelectedIndex = categories.IndexOf(categories.FirstOrDefault(c => c.Cat.Name == "Film"));
                }));

                var user = _client.GetUserDetails(_client.UserId);

                Dispatcher.BeginInvoke((Action) (() =>
                    labelT411.Content =
                        user.Username + " Ratio : " + (user.Uploaded/(double) user.Downloaded).ToString("##.###")));
            };
            worker.RunWorkerCompleted+=delegate(object sender, RunWorkerCompletedEventArgs args)
            {
                gridButton.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Collapsed;
            };

            worker.RunWorkerAsync();
        }

        private void Betaseries_OnClick(object sender, RoutedEventArgs e)
        {
            tc.SelectedIndex = 0;
            LoadBetaseries();
        }

        private void T411_OnClick(object sender, RoutedEventArgs e)
        {
            tc.SelectedIndex = 1;
        }

        private void Freebox_OnClick(object sender, RoutedEventArgs e)
        {
            gridButton.Visibility = Visibility.Collapsed;
            tc.SelectedIndex = 2;
        }
    }
}