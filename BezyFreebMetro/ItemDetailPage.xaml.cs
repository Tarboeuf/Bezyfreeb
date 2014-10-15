using BezyFB;
using BezyFB.EzTv;
using BezyFreebMetro.Common;
using BezyFreebMetro.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page Détail de l'élément, consultez la page http://go.microsoft.com/fwlink/?LinkId=234232

namespace BezyFreebMetro
{
    /// <summary>
    /// Page qui affiche les détails d'un élément appartenant à un groupe.
    /// </summary>
    public sealed partial class ItemDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

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

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation.  Tout état enregistré est également
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
            // TODO: créez un modèle de données approprié pour le domaine posant problème pour remplacer les exemples de données
            //var item = await SampleDataSource.GetItemAsync((String)e.NavigationParameter);
            _Episode = e.NavigationParameter as Episode;
            this.DefaultViewModel["Item"] = e.NavigationParameter;
        }

        private Episode _Episode;

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

        private async void Vu_Click(object sender, RoutedEventArgs e)
        {
            await MainModel.BetaSerie.SetEpisodeSeen(_Episode);
            NavigationHelper.GoBackCommand.Execute(null);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            if (await DownloadMagnet(_Episode))
            {
                await MainModel.DownloadSsTitre(_Episode);
                await MainModel.BetaSerie.SetEpisodeDownnloaded(_Episode);
            }
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> DownloadMagnet(Episode episode)
        {
            if (episode != null)
            {
                var show = await MainModel.Utilisateur.GetSerie(episode);
                if(string.IsNullOrEmpty(show.IdEztv))
                {
                    SettingsShow CustomSettingFlyout = new SettingsShow();
                    CustomSettingFlyout.DataContext = show;
                    CustomSettingFlyout.ShowIndependent();
                }
                if (string.IsNullOrEmpty(show.IdEztv))
                {

                    MessageDialog md = new MessageDialog("La série n'est pas configurée");
                    await md.ShowAsync();
                    return false;
                }
                var magnet = await Eztv.GetMagnetSerieEpisode(show.IdEztv, episode.code);
                if (magnet != null)
                {
                    episode.FileNameSansExtension = magnet.Remove(0, magnet.IndexOf("dn=", System.StringComparison.Ordinal) + 3);
                    episode.FileNameSansExtension = episode.FileNameSansExtension.Remove(episode.FileNameSansExtension.IndexOf("&tr=", System.StringComparison.Ordinal));
                    episode.IdDownload = await MainModel.Freebox.Download(magnet, show.PathFreebox + "/" + (show.ManageSeasonFolder ? episode.season : ""));
                    return true;
                }
            }
            return false;
        }
    }
}