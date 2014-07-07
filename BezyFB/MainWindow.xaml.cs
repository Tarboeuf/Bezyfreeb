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
using BezyFB.BetaSerie;
using BezyFB.EzTv;
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

        public MainWindow()
        {
            InitializeComponent();
            _bs = new BetaSerie.BetaSerie();
            tb.Text = _bs.Error;
            _user = Utilisateur.Current();
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
            MajItemsSource();
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
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
            {
                string pathFreebox = _user.GetSeriePath(episode.show_id, episode.show_title);

                var str = _bs.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    var wc = new WebClient();
                    if (sousTitre != null)
                    {
                        var st = wc.DownloadData(sousTitre);
                        if (sousTitre.EndsWith("zip"))
                        {
                            Stream stream = new MemoryStream(st);
                            st = UnzipFromStream(stream);
                        }

                        string fileName = episode.show_title + "_" + episode.code + ".srt";
                        string pathreseau = pathFreebox + (episode.season == "1" ? "" : episode.season + "/");
                        if (!Directory.Exists(pathreseau))
                            pathreseau = pathFreebox;

                        foreach (var file in Directory.GetFiles(pathreseau))
                        {
                            if (file.Contains(episode.code))
                            {
                                fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                                pathreseau = "";
                            }
                        }

                        File.WriteAllBytes(pathreseau + fileName, st);
                    }
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
                    return zipEntry.ExtraData;
            }
        }

        private void GetMagnetClick(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            var episode = ((Button)sender).CommandParameter as Episode;

            if (episode != null)
            {
                var magnet = Eztv.GetMagnetSerieEpisode(_user.GetIdEztv(episode.show_id, episode.show_title), episode.code);
                Console.WriteLine(magnet);

                Clipboard.SetText(magnet);

                Freebox.Freebox.Download(magnet, Utilisateur.Current().GetSeriePath(episode.show_id, episode.show_title));
            }

            Cursor = Cursors.Arrow;
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            var c = new Configuration.Configuration(_bs);
            c.ShowDialog();
        }
    }
}