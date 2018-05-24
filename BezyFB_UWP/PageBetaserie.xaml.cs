using BezyFB_UWP.Lib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using BetaseriesStandardLib;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageBetaserie : Page
    {
        private rootShowsShow _lastSelectedItem;

        public EpisodeRoot Episodes { get; set; }

        public PageBetaserie()
        {
            this.InitializeComponent();
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            ProgressBarDC.Current.IsProgress = true;
            Episodes = await ClientContext.Current.BetaSerie.GetListeNouveauxEpisodesTest();
            episodes.ItemsSource = Episodes?.shows;
            ProgressBarDC.Current.IsProgress = false;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateForVisualState(AdaptiveStates.CurrentState);

            // Don't play a content transition for first item load.
            // Sometimes, this content will be animated as part of the page transition.
            DisableContentTransitions();
        }
        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), _lastSelectedItem, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(episodes, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
            }
        }

        private void episodes_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (rootShowsShow)e.ClickedItem;
            _lastSelectedItem = clickedItem;

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                // Use "drill in" transition for navigating from master list to detail view
                Frame.Navigate(typeof(DetailPage), clickedItem, new DrillInNavigationTransitionInfo());
            }
            else
            {
                // Play a refresh animation when the user switches detail items.
                EnableContentTransitions();
            }
        }
        private void EnableContentTransitions()
        {
            DetailContentPresenter.ContentTransitions.Clear();
            DetailContentPresenter.ContentTransitions.Add(new EntranceThemeTransition());
        }

        private void DisableContentTransitions()
        {
            if (DetailContentPresenter != null)
            {
                DetailContentPresenter.ContentTransitions.Clear();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ProgressBarDC.Current.IsProgress = true;
            Episodes = await ClientContext.Current.BetaSerie.GetListeNouveauxEpisodesTest();
            episodes.ItemsSource = Episodes?.shows;
            ProgressBarDC.Current.IsProgress = false;
        }
    }
}
