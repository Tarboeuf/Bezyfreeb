using BezyFB;
using BezyFB.BetaSerie;
using BezyFB.Configuration;
using BezyFB.Freebox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// Le modèle de données défini par ce fichier sert d'exemple représentatif d'un modèle fortement typé
// modèle.  Les noms de propriétés choisis correspondent aux liaisons de données dans les modèles d'élément standard.
//
// Les applications peuvent utiliser ce modèle comme point de départ et le modifier à leur convenance, ou le supprimer complètement et
// le remplacer par un autre correspondant à leurs besoins. L'utilisation de ce modèle peut vous permettre d'améliorer votre application 
// réactivité en lançant la tâche de chargement des données dans le code associé à App.xaml lorsque l'application 
// est démarrée pour la première fois.

namespace BezyFreebMetro.Data
{
    /// <summary>
    /// Modèle de données d'élément générique.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Modèle de données de groupe générique.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }


    /// <summary>
    /// Crée une collection de groupes et d'éléments dont le contenu est lu à partir d'un fichier json statique.
    /// 
    /// SampleDataSource initialise avec les données lues à partir d'un fichier json statique dans 
    /// projet.  Elle fournit des exemples de données à la fois au moment de la conception et de l'exécution.
    /// </summary>
    public sealed class MainModel
    {
        private static BetaSerie _BetaSerie = new BetaSerie();
        private static Utilisateur _Utilisateur = Utilisateur.Current();
        private static Freebox _Freebox = new Freebox();

        public static BetaSerie BetaSerie { get { return _BetaSerie; } }
        public static Freebox Freebox { get { return _Freebox; } }
        public static Utilisateur Utilisateur { get { return _Utilisateur; } }

        private ObservableCollection<rootShowsShow> _groups = new ObservableCollection<rootShowsShow>();
        public ObservableCollection<rootShowsShow> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<rootShowsShow>> GetGroupsAsync()
        {
            var root = await _BetaSerie.GetListeNouveauxEpisodesTest();
            if (null != root)
                return root.shows;
            return null;
        }

        private static string ExtractEncoding(string movieFilePath)
        {
            if (movieFilePath.Contains("LOL"))
                return "LOL";
            if (movieFilePath.Contains("2HD"))
                return "2HD";
            return "";
        }

        public static async Task DownloadSsTitre(Episode episode)
        {
            if (episode != null)
            {
                var userShow = await Utilisateur.GetSerie(episode);
                string pathFreebox = userShow.PathReseau;

                var str = await BetaSerie.GetPathSousTitre(episode.id);
                if (str.subtitles.Any())
                {
                    string fileName = episode.show_title + "_" + episode.code + ".srt";

                    string encoding = "";
                    if (!string.IsNullOrEmpty(episode.IdDownload))
                    {
                        string file = await Freebox.GetFileNameDownloaded(episode.IdDownload);

                        if (!string.IsNullOrEmpty(file))
                        {
                            if (file.LastIndexOf('.') <= (file.Length - 5))
                                fileName = file + ".srt";
                            else
                                fileName = file.Replace(file.Substring(file.LastIndexOf('.')), ".srt");
                        }
                    }
                    string pathreseau = pathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : "");

                    {
                        if (string.IsNullOrEmpty(episode.IdDownload))
                        {
                            var lst = await Freebox.Ls(AppSettings.Default.PathVideo + "/" + userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""));
                            string f = lst.FirstOrDefault(s => s.Contains(episode.code) && !s.EndsWith(".srt"));
                            if (null != f)
                            {
                                fileName = f.Replace(f.Substring(f.LastIndexOf('.')), ".srt");
                            }
                        }

                        //Process.Start(pathreseau);
                    }
                    encoding = ExtractEncoding(fileName);
                    var sousTitre = str.subtitles.OrderByDescending(c => c.quality).Select(s => s.url).FirstOrDefault();

                    //var wc = new WebClient();
                    if (sousTitre != null)
                    {
                        HttpClient client = new HttpClient();

                        var st = await client.GetByteArrayAsync(sousTitre);
                        //var st = wc.DownloadData(sousTitre);

                        string message = null;
                        try
                        {
                            Stream stream = new MemoryStream(st);
                            var st2 = UnzipFromStream(stream, encoding);
                            if (st2 != null)
                                st = st2;
                        }
                        catch (Exception ex)
                        {
                            message = ex.Message;
                        }
                        if (null != message)
                        {
                            MessageDialog md = new MessageDialog(message);
                            await md.ShowAsync();
                        }

                        await Freebox.UploadFile(pathreseau + fileName, userShow.PathFreebox + "/" + (userShow.ManageSeasonFolder ? episode.season : ""), fileName, System.Text.Encoding.UTF8.GetString(st, 0, st.Length));
                    }
                }
                else
                {
                    MessageDialog md = new MessageDialog("Aucun sous titre disponible");
                    await md.ShowAsync();
                }
            }
        }

        private static byte[] UnzipFromStream(Stream zipStream, string encoding)
        {
            ZipArchive za = new ZipArchive(zipStream);
            foreach (var item in za.Entries)
            {
                String entryFileName = item.FullName;

                if (entryFileName.Contains(".srt") && entryFileName.Contains(encoding))
                {
                    int file_size = (int)item.Length;
                    byte[] blob = new byte[file_size];
                    int bytes_read = 0;
                    int offset = 0;

                    var open = item.Open();

                    while ((bytes_read = open.Read(blob, 0, file_size)) != 0)
                    {
                        offset += bytes_read;
                    }

                    //closing every thing
                    return blob;
                }
            }
            foreach (var item in za.Entries)
            {
                String entryFileName = item.FullName;

                if (entryFileName.Contains(".srt"))
                {
                    int file_size = (int)item.Length;
                    byte[] blob = new byte[file_size];
                    int bytes_read = 0;
                    int offset = 0;

                    while ((bytes_read = item.Open().Read(blob, 0, file_size)) != 0)
                    {
                        offset += bytes_read;
                    }

                    //closing every thing
                    return blob;
                }
            }

            return new byte[0];
        }
    }
}