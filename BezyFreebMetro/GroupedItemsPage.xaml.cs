using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Web.Http;
using BezyFB;
using BezyFB.Configuration;
using BezyFB.Freebox;
using BezyFreebMetro.BezyFreeb.IMDB;
using BezyFreebMetro.Common;
using BezyFreebMetro.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page Éléments groupés, consultez la page http://go.microsoft.com/fwlink/?LinkId=234231

namespace BezyFreebMetro
{
    /// <summary>
    /// Page affichant une collection groupée d'éléments.
    /// </summary>
    public sealed partial class GroupedItemsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static bool _IsLoaded = false;
        private string _CurrentPath = null;
        private static IEnumerable<rootShowsShow> _EpisodesBetaSerie;

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

        public GroupedItemsPage()
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (_IsLoaded)
            {
                DefaultViewModel["Groups"] = _EpisodesBetaSerie;
                ProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            _IsLoaded = true;
            ProgressBar.Visibility = Visibility.Visible;


            _EpisodesBetaSerie = await MainModel.GetGroupsAsync();

            DefaultViewModel["Groups"] = _EpisodesBetaSerie;
            if (null == _EpisodesBetaSerie)
            {
                TextBlockErreur.Visibility = Visibility.Visible;
                return;
            }

            TextBlockErreur.Visibility = Visibility.Collapsed;

            foreach (var item in _EpisodesBetaSerie)
            {
                var path = await ImdbAPI.GetImagePath(item.thetvdb_id);
                if (null != path)
                    item.ImagePath = new BitmapImage(new System.Uri(path));
            }
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Invoqué lorsqu'un utilisateur clique sur un en-tête de groupe.
        /// </summary>
        /// <param name="sender">Button utilisé en tant qu'en-tête pour le groupe sélectionné.</param>
        /// <param name="e">Données d'événement décrivant la façon dont le clic a été initié.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Déterminez le groupe représenté par l'instance Button
            var group = (sender as FrameworkElement).DataContext;

            // Accédez à la page de destination souhaitée, puis configurez la nouvelle page
            // en transmettant les informations requises en tant que paramètre de navigation.
            this.Frame.Navigate(typeof(GroupDetailPage), (rootShowsShow)group);
        }

        /// <summary>
        /// Invoqué lorsqu'un utilisateur clique sur un élément appartenant à un groupe.
        /// </summary>
        /// <param name="sender">GridView (ou ListView lorsque l'état d'affichage de l'application est Snapped)
        /// affichant l'élément sur lequel l'utilisateur a cliqué.</param>
        /// <param name="e">Données d'événement décrivant l'élément sur lequel l'utilisateur a cliqué.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Accédez à la page de destination souhaitée, puis configurez la nouvelle page
            // en transmettant les informations requises en tant que paramètre de navigation.
            //var itemId = ((rootShowsShow)e.ClickedItem);
            this.Frame.Navigate(typeof(GroupDetailPage), e.ClickedItem);
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

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings CustomSettingFlyout = new Settings();
            CustomSettingFlyout.Show();
        }

        private async void Freebox_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            itemGridView.Visibility = Visibility.Collapsed;
            itemGridViewFreebox.Visibility = Visibility.Visible;
            itemGridViewPlanning.Visibility = Visibility.Collapsed;

            Freebox fb = new Freebox();
            _CurrentPath = AppSettings.Default.PathVideo;
            DefaultViewModel["FreeboxFolder"] = await fb.Ls(AppSettings.Default.PathVideo);
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void BezyFreeb_OnClick(object sender, RoutedEventArgs e)
        {
            itemGridView.Visibility = Visibility.Visible;
            itemGridViewFreebox.Visibility = Visibility.Collapsed;
            itemGridViewPlanning.Visibility = Visibility.Collapsed;
        }


        private async void UpdateFreebox_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            Freebox fb = new Freebox();
            _CurrentPath += "/" + (sender as Button).Content;
            DefaultViewModel["FreeboxFolder"] = await fb.Ls(_CurrentPath);
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private async void Planning_OnClick(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            itemGridView.Visibility = Visibility.Collapsed;
            itemGridViewFreebox.Visibility = Visibility.Collapsed;
            itemGridViewPlanning.Visibility = Visibility.Visible;

            DefaultViewModel["Planning"] = (await MainModel.GetPlanning()).episodes.Where(ep => ep.Date >= DateTime.Now.Date && ep.Date < DateTime.Now.AddDays(6)).GroupBy(g => g.Date.DayOfWeek).Select(r => new PlanningJour(r)).OrderBy(p => p.DayOfWeek, new PlanningJour());
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    public class PlanningJour : IComparer<DayOfWeek>
    {
        public PlanningJour()
        {
            
        }

        public PlanningJour(IGrouping<DayOfWeek, Episode> group)
        {
            Items = new ObservableCollection<Episode>(group);
            Jour = group.Key.ToString();
            DayOfWeek = group.Key;
        }

        public DayOfWeek DayOfWeek { get; set; }

        public string Jour { get; set; }

        public ObservableCollection<Episode> Items { get; set; }

        #region Implementation of IComparer<in DayOfWeek>
        
        /// <summary>
        /// Retourne un code de hachage pour l'objet spécifié.
        /// </summary>
        /// <returns>
        /// Code de hachage pour l'objet spécifié.
        /// </returns>
        /// <param name="obj"><see cref="T:System.Object"/> pour lequel un code de hachage doit être retourné.</param><exception cref="T:System.ArgumentNullException">Le type de <paramref name="obj"/> est un type référence et <paramref name="obj"/> est null.</exception>
        public int GetHashCode(DayOfWeek obj)
        {
            return obj.GetHashCode();
        }


        /// <summary>
        /// Compare deux objets et retourne une valeur indiquant si le premier est inférieur, égal ou supérieur au second.
        /// </summary>
        /// <returns>
        /// Entier signé qui indique les valeurs relatives de <paramref name="x"/> et <paramref name="y"/>, comme indiqué dans le tableau suivant. 
        /// Valeur  	Signification  	
        /// Inférieur à zéro 	<paramref name="x"/> est inférieur à <paramref name="y"/>. 	
        /// Zéro 	<paramref name="x"/> est égal à <paramref name="y"/>. 	
        /// Supérieure à zéro 	<paramref name="x"/> est supérieur à <paramref name="y"/>.
        /// </returns>
        /// <param name="x">Premier objet à comparer.</param><param name="y">Second objet à comparer.</param>
        public int Compare(DayOfWeek x, DayOfWeek y)
        {
            if (x == y)
                return 0;

            if (x == DayOfWeek.Sunday)
                return 1;

            if (y == DayOfWeek.Sunday)
                return -1;

            return (int)x - (int)y;
        }

        #endregion
    }

    public class DateConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((DateTime)value).ToString("d MMM yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}