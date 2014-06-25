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
using ICSharpCode.SharpZipLib.Zip;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BetaSerie _bs;
        private readonly Utilisateur _user;

        public MainWindow()
        {
            InitializeComponent();
            _bs = new BetaSerie();
            tb.Text = _bs.Error;
            _user = Utilisateur.Current();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var shows = _bs.GetListeNouveauxEpisodesTest();
            if (null != shows)
                tv.ItemsSource = shows.shows;
            tb.Text = _bs.Error;
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
                        string pathreseau = pathFreebox + episode.season + "/";
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
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;

                if (entryFileName.Contains(".srt"))
                    return zipEntry.ExtraData;
            }
            return new byte[0];
        }

        private void Eztv_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(Eztv.GetMagnetSerieEpisode("40", "S07E10"));
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

                Freebox.Download(magnet, Utilisateur.Current().GetSeriePath(episode.show_id, episode.show_title));
            }

            Cursor = Cursors.Arrow;
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
        }

        private void FB_Click(object sender, RoutedEventArgs e)
        {
            Freebox.GenererToken();
        }
    }
}