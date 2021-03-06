﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BetaseriesStandardLib;
using BezyFB.Configuration;
using BezyFB.Helpers;
using BezyFB.Properties;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour BetaSerieUserControl.xaml
    /// </summary>
    public partial class BetaSerieUserControl
    {
        public BetaSerieUserControl()
        {
            InitializeComponent();
        }

        private Lazy<Utilisateur> _user = new Lazy<Utilisateur>(Utilisateur.Current);

        public void InitialiseElements()
        {
            _user = new Lazy<Utilisateur>(Utilisateur.Current);
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
                        var lst = await ClientContext.Current.Freebox.Ls(Settings.Default.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), false, true);
                        string f = lst?.FirstOrDefault(s => s.Contains(episode.code) && !s.EndsWith(".srt"));
                        if (f != null)
                        {
                            fileName = f.Replace(f.Substring(f.LastIndexOf('.')), ".srt");
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

                    if (sousTitre != null)
                    {
                        var result = await DownloadSousTitreHelper.DownloadAndUnzip(encoding, sousTitre);
                        if (!result.IsOk)
                        {
                            await ClientContext.Current.MessageDialogService.AfficherMessage(result.Exception.Message);
                            Cursor = Cursors.Arrow;
                            return false;
                        }
                        byte[] st = result.Result;

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
                    episode.IdDownload = await ClientContext.Current.Freebox.Download(magnet, serie.PathFreebox + "/" + (serie.ManageSeasonFolder ? episode.season : ""), true);
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

            if (movieFilePath.ToUpper().Contains("X264-") && movieFilePath.ToUpper().Contains("[EZTV]"))
            {
                int idDebut = movieFilePath.IndexOf("X264-", StringComparison.CurrentCultureIgnoreCase) + 5;
                return movieFilePath.Substring(idDebut,
                    movieFilePath.IndexOf("[EZTV]", StringComparison.CurrentCultureIgnoreCase) - idDebut);
            }
            if (movieFilePath.ToUpper().Contains("X264-"))
            {
                int idDebut = movieFilePath.IndexOf("X264-", StringComparison.CurrentCultureIgnoreCase) + 5;
                return movieFilePath.Substring(idDebut, movieFilePath.Length - idDebut - 4);
            }

            if (movieFilePath.Contains("LOL"))
                return "LOL";
            if (movieFilePath.Contains("2HD"))
                return "2HD";
            if (movieFilePath.Contains("FQM"))
                return "FQM";
            if (movieFilePath.Contains("TURBO"))
                return "TURBO";
            return "";
        }

        public async Task LoadBetaseries()
        {
            Window.pb.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(ClientContext.Current.BetaSerie.Error))
                SetStatusText(ClientContext.Current.BetaSerie.Error);
            else
                SetStatusText("Récupération des épisodes depuis BetaSeries");

            var root = await ClientContext.Current.BetaSerie.GetListeNouveauxEpisodesTest();

            if (root != null)
                tv.ItemsSource = root.shows;
            Window.gridButton.Visibility = Visibility.Collapsed;
            Window.pb.Visibility = Visibility.Collapsed;
            TextBlockResteAVoir.Text = root.shows.Select(s => s.unseen.Count()).Sum().ToString();
            SetStatusText("Episodes récupérés");
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

        private async void VoirToutesLesSeries_OnClick(object sender, RoutedEventArgs e)
        {
            Window.pb.Visibility = Visibility.Visible;
            if (!string.IsNullOrEmpty(ClientContext.Current.BetaSerie.Error))
                SetStatusText(ClientContext.Current.BetaSerie.Error);
            else
                SetStatusText("Récupération des séries depuis BetaSeries");

            var root = await ClientContext.Current.BetaSerie.GetListeSeries();

            if (root != null)
                tv.ItemsSource = root;
            Window.gridButton.Visibility = Visibility.Collapsed;
            Window.pb.Visibility = Visibility.Collapsed;
            SetStatusText("Séries récupérés");
        }
    }

}
