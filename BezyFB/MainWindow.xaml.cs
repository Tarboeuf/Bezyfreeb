using BezyFB.BetaSerieLib;
using BezyFB.Configuration;
using BezyFB.EzTv;
using BezyFB.Helpers;
using BezyFB.Properties;
using BezyFB.T411;
using FreeboxPortableLib;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using WpfTemplateBaseLib;
using WpfTemplateLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Lazy<Utilisateur> _user = new Lazy<Utilisateur>(Utilisateur.Current);

        public MainWindow()
        {
            InitializeComponent();
            SetStatusText("Veuillez choisir votre catégorie");
            gridButton.Visibility = Visibility.Visible;
            T411Client.BaseAddress = Settings.Default.T411Address;
            CheckConfiguration();
        }

        public void InitialiseElements()
        {
            _user = new Lazy<Utilisateur>(Utilisateur.Current);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await LoadBetaseries();
        }

        private async void SetDl(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
                await ClientContext.Current.BetaSerie.SetEpisodeDownnloaded(episode);
            Cursor = Cursors.Arrow;
        }

        private async void SetSetSeen(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            new NoteWindow(ClientContext.Current.BetaSerie, episode).ShowDialog();

            if (episode != null)
                await ClientContext.Current.BetaSerie.SetEpisodeSeen(episode);
            Cursor = Cursors.Arrow;
        }

        private async void DlTout(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (await DownloadMagnet(episode) && await DownloadSsTitre(episode))
                await ClientContext.Current.BetaSerie.SetEpisodeDownnloaded(episode);
            Cursor = Cursors.Arrow;
        }

        private async void DlStClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            await DownloadSsTitre(episode);
        }

        private string ExtractEncoding(string movieFilePath)
        {
            if (movieFilePath.Contains("LOL"))
                return "LOL";
            if (movieFilePath.Contains("2HD"))
                return "2HD";
            return "";
        }

        private async Task<bool> DownloadSsTitre(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var userShow = await _user.Value.GetSerie(episode);
                string pathFreebox = userShow.PathReseau;

                var str = await ClientContext.Current.BetaSerie.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    string fileName = episode.show_title + "_" + episode.code + ".srt";

                    if (!string.IsNullOrEmpty(episode.IdDownload))
                    {
                        string file = await ClientContext.Current.Freebox.GetFileNameDownloaded(episode.IdDownload);

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
                        var lst = await ClientContext.Current.Freebox.Ls(Settings.Default.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), false);
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
                        pathreseau = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
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
                            await ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message);
                            Cursor = Cursors.Arrow;
                            return false;
                        }

                        File.WriteAllBytes(pathreseau + fileName, st);
                        try
                        {
                            await ClientContext.Current.Freebox.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName);
                        }
                        catch (Exception ex)
                        {
                            await ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message);
                        }
                        await ClientContext.Current.Freebox.CleanUpload();
                        File.Delete(pathreseau + fileName);

                        SetStatusText("Fichier : " + fileName);
                    }
                }
                else
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("Aucun sous titre disponible");
                    Cursor = Cursors.Arrow;
                    return false;
                }
            }
            Cursor = Cursors.Arrow;
            return true;
        }

        private void SetStatusText(string text)
        {
            var window = Notification.FindAncestor<Window>();
            if (window == null) return;
            Notification.AddNotification(text);
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

        private async void GetMagnetClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            await DownloadMagnet(episode);
        }

        private async Task<bool> DownloadMagnet(Episode episode)
        {
            Cursor = Cursors.Wait;
            if (episode != null)
            {
                var serie = await _user.Value.GetSerie(episode);
                var magnet = await ClientContext.Current.Eztv.GetMagnetSerieEpisode(serie.IdEztv, episode.code);
                if (magnet != null)
                    episode.IdDownload = await ClientContext.Current.Freebox.Download(magnet, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""));
                else if (serie.IdEztv == null)
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage("Serie " + serie.ShowName + " + non configurée");
                    Cursor = Cursors.Arrow;
                    return false;
                }
                else
                {
                    // try to get torrent file.
                    var torrentStream = await ClientContext.Current.Eztv.GetTorrentSerieEpisode(serie.IdEztv, episode.code);

                    if (null != torrentStream)
                    {
                        episode.IdDownload = await ClientContext.Current.Freebox.DownloadFile(torrentStream, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""), true);
                    }
                    else
                    {
                        await ClientContext.Current.MessageDialogService.AfficherMessage("Episode " + episode.code + " de la serie " + serie.ShowName + " non trouvé");
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
            var c = new Configuration.Configuration();
            c.Owner = this;
            c.ShowDialog();
            InitialiseElements();
            CheckConfiguration();
        }

        private async void Download_All_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir tout télécharger ?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            var root = await ClientContext.Current.BetaSerie.GetListeNouveauxEpisodesTest();

            foreach (var rootShowsShow in root.shows)
            {
                foreach (var episode in rootShowsShow.unseen)
                {
                    try
                    {
                        await DownloadMagnet(episode);
                    }
                    catch (Exception ex)
                    {
                        await ClientContext.Current.MessageDialogService.AfficherMessage(episode.show_title + "(" + episode.code + ") : " + ex.Message + "\r\n");
                    }

                    try
                    {
                        await DownloadSsTitre(episode);
                    }
                    catch (Exception ex)
                    {
                        await ClientContext.Current.MessageDialogService.AfficherMessage(episode.show_title + "(" + episode.code + ") : " + ex.Message + "\r\n");
                    }
                }
            }

            Mouse.OverrideCursor = null;
            if (!string.IsNullOrEmpty(((MessageDialogService)ClientContext.Current.MessageDialogService).MessageBuffer))
            {
                MessageBox.Show(((MessageDialogService)ClientContext.Current.MessageDialogService).MessageBuffer);
                ((MessageDialogService)ClientContext.Current.MessageDialogService).MessageBuffer = string.Empty;
            }
        }

        private async void MainWindow_OnClosed(object sender, EventArgs e)
        {
            await ClientContext.Current.Freebox.Deconnexion();
        }

        private async void SettingsClick(object sender, RoutedEventArgs e)
        {
            var s = ((Button)sender).CommandParameter as rootShowsShow;

            if (s != null)
            {
                if ((new WindowShow { DataContext = await _user.Value.GetSerie(s) }).ShowDialog() ?? false)
                {
                    _user.Value.SerializeElement();
                }
            }
        }

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tc.SelectedIndex == 1 && lv.ItemsSource == null)
            {
                pb.Visibility = Visibility.Visible;
                SetStatusText("Chargement depuis T411");
                await LoadT411();
                SetStatusText("T411 chargé");
                pb.Visibility = Visibility.Collapsed;
            }
            if (tc.SelectedIndex == 2)
            {
                if (!(TabFreebox.DataContext is UserFreebox))
                {
                    try
                    {
                        var uf = await ClientContext.Current.Freebox.GetInfosFreebox();
                        TabFreebox.DataContext = uf;

                        //await uf.LoadMovies(Dispatcher);
                    }
                    catch (Exception ex)
                    {
                        await ClientContext.Current.MessageDialogService.AfficherMessage("Erreur lors de la récupération des infos freebox : \r\n" + ex.Message);
                    }
                }
            }
        }

        private async void ButtonTelechargerTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    using (var stream = ClientContext.Current.T411.DownloadTorrent(torrent.Torrent.Id))
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
                            await ClientContext.Current.Freebox.DownloadFile(stream, torrent.Name + ".torrent", Settings.Default.PathFilm, false);
                        }
                    }
                }
            }
        }

        private async void ButtonExtraInfoOnClick(object sender, RoutedEventArgs e)
        {
            pb.Visibility = Visibility.Visible;
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    await torrent.Initialiser();
                }
            }
            SetStatusText("Extra infos récupérées");
            pb.Visibility = Visibility.Collapsed;
        }

        private async void SupprimerFilm_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = ((Button)sender).DataContext as OMDb;
            if (null != dc)
            {
                await ClientContext.Current.Freebox.DeleteFile(Settings.Default.PathFilm + dc.FileName);
            }
        }

        private async void buttonT411Rechercher_Click(object sender, RoutedEventArgs e)
        {
            pb.Visibility = Visibility.Visible;
            List<Torrent> items = null;
            if (string.IsNullOrEmpty(textBoxRechercheT411.Text))
            {
                if (GetTop100.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top 100");
                    items = (await ClientContext.Current.T411.GetTop100());
                }
                else if (GetTopMonth.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top month");
                    items = (await ClientContext.Current.T411.GetTopMonth());
                }
                else if (GetTopWeek.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top week");
                    items = (await ClientContext.Current.T411.GetTopWeek());
                }
                else if (GetTopToday.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top today");
                    items = (await ClientContext.Current.T411.GetTopToday());
                }
                if (AvecCategorie.IsChecked == true)
                    items = items.Where(t => t.CategoryName == ((SousCategorie)comboCategoryT411.SelectedValue).Cat.Name).ToList();
            }
            else
            {
                SetStatusText("Connecté t411 recherche par nom");
                if (AvecCategorie.IsChecked == true)
                {
                    items = (await ClientContext.Current.T411.GetQuery(string.Format("{0}", HttpUtility.UrlEncode(textBoxRechercheT411.Text)),
                    new QueryOptions { CategoryIds = new List<int> { ((SousCategorie)comboCategoryT411.SelectedValue).Cat.Id }, Limit = 1000 })).Torrents;
                }
                else
                {
                    items = (await ClientContext.Current.T411.GetQuery(string.Format("{0}", HttpUtility.UrlEncode(textBoxRechercheT411.Text)))).Torrents;
                }
            }

            lv.ItemsSource = items.OrderByDescending(t => t.Times_completed).Select(t => new MyTorrent(t, ClientContext.Current.GuessIt, ClientContext.Current.ApiConnector));
            SetStatusText("torrents récupéré");
            pb.Visibility = Visibility.Collapsed;
        }

        private void Quitter_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task LoadBetaseries()
        {
            pb.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(ClientContext.Current.BetaSerie.Error))
                SetStatusText(ClientContext.Current.BetaSerie.Error);
            else
                SetStatusText("Récupération des épisodes depuis BetaSeries");

            var root = await ClientContext.Current.BetaSerie.GetListeNouveauxEpisodesTest();

            if (root != null)
                tv.ItemsSource = root.shows;
            gridButton.Visibility = Visibility.Collapsed;
            pb.Visibility = Visibility.Collapsed;
            TextBlockResteAVoir.Text = root.shows.Select(s => s.unseen.Count()).Sum().ToString();
            SetStatusText("Episodes récupérés");
        }

        private async Task LoadT411()
        {
            var worker = new BackgroundWorker();
            pb.Visibility = Visibility.Visible;
            try
            {
                var topWeek = await ClientContext.Current.T411.GetTopWeek();
                await Dispatcher.BeginInvoke((Action)(() => lv.ItemsSource =
                       topWeek
                        .OrderByDescending(t => t.Times_completed)
                        .Select(t => new MyTorrent(t, ClientContext.Current.GuessIt, ClientContext.Current.ApiConnector))));

                var categories = new List<SousCategorie>();
                foreach (var category1 in (await ClientContext.Current.T411.GetCategory()))
                {
                    foreach (var cat in category1.Value.Cats)
                    {
                        categories.Add(new SousCategorie(cat.Value, category1.Value.Name));
                    }
                }

                await Dispatcher.BeginInvoke((Action)(() =>
                {
                    comboCategoryT411.ItemsSource = categories;
                    comboCategoryT411.SelectedIndex = categories.IndexOf(categories.FirstOrDefault(c => c.Cat.Name == "Film"));
                }));

                var user = await ClientContext.Current.T411.GetUserDetails(ClientContext.Current.T411.UserId);
                await Dispatcher.BeginInvoke((Action)(() =>
                    labelT411.Content =
                        user.Username + " Ratio : " + (user.Uploaded / (double)user.Downloaded).ToString("##.###")));

                gridButton.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Collapsed;

                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                await ClientContext.Current.MessageDialogService.AfficherMessage("Impossible de charger les données de T411.\r\n" + ex.Message);
            }
        }

        private async void Betaseries_OnClick(object sender, RoutedEventArgs e)
        {
            tc.SelectedIndex = 0;
            await LoadBetaseries();
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

        private async void ChargerTout_OnClick(object sender, RoutedEventArgs e)
        {
            var s = ((Button)sender).CommandParameter as rootShowsShow;

            if (s != null)
            {
                var episodes = await ClientContext.Current.BetaSerie.GetShowEpisode(s);
                s.unseen = episodes.ToArray();
            }
        }

        private async void SetSetUnSeen(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
                await ClientContext.Current.BetaSerie.SetEpisodeUnSeen(episode);
            Cursor = Cursors.Arrow;
        }

        private void CheckConfiguration()
        {
            ButtonBetaserie.IsEnabled = !string.IsNullOrEmpty(Settings.Default.LoginBetaSerie) &&
                                        !string.IsNullOrEmpty(Settings.Default.PwdBetaSerie);
            ButtonT411.IsEnabled = !string.IsNullOrEmpty(Settings.Default.LoginT411) &&
                                        !string.IsNullOrEmpty(Settings.Default.PassT411);
            ButtonFreebox.IsEnabled = !string.IsNullOrEmpty(Settings.Default.IpFreebox) &&
                                        !Settings.Default.IpFreebox.Contains("freebox");

            LabelAvertissement.Visibility = ButtonBetaserie.IsEnabled && ButtonT411.IsEnabled && ButtonBetaserie.IsEnabled
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void FreeSpace_OnClick(object sender, RoutedEventArgs e)
        {
            TailleDossierFreebox window = new TailleDossierFreebox();
            window.ShowDialog();
        }

        private void ExporterConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Fichier config (*.confbz)|*.confbz";
                if (sfd.ShowDialog(this) ?? false)
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                    config.SaveAs(sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message + ex.StackTrace);
            }
        }

        private void ImporterConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = "Fichier config (*.confbz)|*.confbz";
                if (ofd.ShowDialog(this) ?? false)
                {
                    try
                    {
                        // Open settings file as XML
                        var import = XDocument.Load(ofd.FileName);
                        // Get the <setting> elements
                        var settings = import.XPathSelectElements("//setting");
                        foreach (var setting in settings)
                        {
                            string name = setting.Attribute("name").Value;
                            string value = setting.XPathSelectElement("value").FirstNode.ToString();

                            try
                            {
                                Settings.Default[name] = value; // throws SettingsPropertyNotFoundException
                            }
                            catch (SettingsPropertyNotFoundException spnfe)
                            {
                                //_logger.WarnException("An imported setting ({0}) did not match an existing setting.".FormatString(name), spnfe);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        //_logger.ErrorException("Could not import settings.", exc);
                        Settings.Default.Reload(); // from last set saved, not defaults
                    }
                }
            }
            catch (Exception ex)
            {
                ClientContext.Current.MessageDialogService.AfficherMessage(ex.Message + ex.StackTrace);
            }
        }
    }
}