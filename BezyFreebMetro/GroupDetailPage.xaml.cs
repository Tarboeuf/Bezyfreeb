using System.Collections.ObjectModel;
using BezyFB;
using BezyFreebMetro.Common;
using BezyFreebMetro.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page Détail du groupe, consultez la page http://go.microsoft.com/fwlink/?LinkId=234229

namespace BezyFreebMetro
{
    /// <summary>
    /// Page affichant une vue d'ensemble d'un groupe, ainsi qu'un aperçu des éléments
    /// qu'il contient.
    /// </summary>
    public sealed partial class GroupDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private IEnumerable<SaisonVM> _Episodes;

        /// <summary>
        /// NavigationHelper est utilisé sur chaque page pour faciliter la navigation et 
        /// gestion de la durée de vie des processus
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Cela peut être remplacé par un modèle d'affichage fortement typé.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }


        public GroupDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="sender">
        /// La source de l'événement ; en général <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Données d'événement qui fournissent le paramètre de navigation transmis à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page et
        /// un dictionnaire d'état conservé par cette page durant une session
        /// antérieure.  L'état n'aura pas la valeur Null lors de la première visite de la page.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var show = (rootShowsShow)e.NavigationParameter;
            this.DefaultViewModel["Group"] = show;
            _Episodes = show.unseen.GroupBy(ep => ep.season).Select(g => new SaisonVM(g));
            this.DefaultViewModel["Groups"] = _Episodes;
        }

        /// <summary>
        /// Invoqué lorsqu'un utilisateur clique sur un élément.
        /// </summary>
        /// <param name="sender">GridView qui affiche l'élément sur lequel l'utilisateur a cliqué.</param>
        /// <param name="e">Données d'événement décrivant l'élément sur lequel l'utilisateur a cliqué.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Accédez à la page de destination souhaitée, puis configurez la nouvelle page
            // en transmettant les informations requises en tant que paramètre de navigation.
            this.Frame.Navigate(typeof(ItemDetailPage), e.ClickedItem);
        }

        #region Inscription de NavigationHelper

        /// Les méthodes fournies dans cette section sont utilisées simplement pour permettre
        /// NavigationHelper pour répondre aux méthodes de navigation de la page.
        /// 
        /// La logique spécifique à la page doit être placée dans les gestionnaires d'événements pour  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// et <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// Le paramètre de navigation est disponible dans la méthode LoadState 
        /// en plus de l'état de page conservé durant une session antérieure.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void EpisodeSeenClick(object sender, RoutedEventArgs e)
        {
            var episode = ((Button)sender).CommandParameter as Episode;
            await MainModel.BetaSerie.SetEpisodeSeen(episode);
            foreach (var saisonVm in _Episodes)
            {
                saisonVm.Items.Remove(episode);
            }
            ((Button)sender).Background = new SolidColorBrush(Colors.DeepPink);
            this.DefaultViewModel["Groups"] = _Episodes;
        }
    }

    public class SaisonVM : INotifyPropertyChanged
    {
        private IGrouping<string, Episode> _Group;
        private ObservableCollection<Episode> _Items;

        public SaisonVM(IGrouping<string, Episode> group)
        {
            _Group = group;
            _Items = new ObservableCollection<Episode>(group);
            _Items.CollectionChanged += delegate { OnPropertyChanged("Items"); };
        }

        public string Nom
        {
            get
            {
                return _Group.Key;
            }
        }

        public ObservableCollection<Episode> Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                OnPropertyChanged("Items");
            }
        }

        private void OnPropertyChanged(string value)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(value));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (Object.Equals("1", value))
                return new SolidColorBrush(Colors.DeepPink);
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}