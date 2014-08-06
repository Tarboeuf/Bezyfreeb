using BezyFB;
using BezyFB.BetaSerie;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
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
    public sealed class SampleDataSource
    {
        private static BetaSerie _BetaSerie = new BetaSerie();

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

        //public static async Task<Episode> GetGroupAsync(string uniqueId)
        //{
        //    //await _BetaSerie.GetSampleDataAsync();
        //    //// Une simple recherche linéaire est acceptable pour les petits groupes de données
        //    //var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
        //    //if (matches.Count() == 1) return matches.First();
        //    //return null;
        //}

        //public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        //{
        //    await _sampleDataSource.GetSampleDataAsync();
        //    // Une simple recherche linéaire est acceptable pour les petits groupes de données
        //    var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
        //    if (matches.Count() == 1) return matches.First();
        //    return null;
        //}

        //private async Task GetSampleDataAsync()
        //{
        //    if (this._groups.Count != 0)
        //        return;

        //    Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");

        //    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
        //    string jsonText = await FileIO.ReadTextAsync(file);
        //    JsonObject jsonObject = JsonObject.Parse(jsonText);
        //    JsonArray jsonArray = jsonObject["Groups"].GetArray();

        //    foreach (JsonValue groupValue in jsonArray)
        //    {
        //        JsonObject groupObject = groupValue.GetObject();
        //        SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
        //                                                    groupObject["Title"].GetString(),
        //                                                    groupObject["Subtitle"].GetString(),
        //                                                    groupObject["ImagePath"].GetString(),
        //                                                    groupObject["Description"].GetString());

        //        foreach (JsonValue itemValue in groupObject["Items"].GetArray())
        //        {
        //            JsonObject itemObject = itemValue.GetObject();
        //            group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
        //                                               itemObject["Title"].GetString(),
        //                                               itemObject["Subtitle"].GetString(),
        //                                               itemObject["ImagePath"].GetString(),
        //                                               itemObject["Description"].GetString(),
        //                                               itemObject["Content"].GetString()));
        //        }
        //        this.Groups.Add(group);
        //    }
        //}
    }
}