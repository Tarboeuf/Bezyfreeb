using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using BezyFB_UWP.Lib;
using BezyFB_UWP.Lib.Helpers;
using BezyFB_UWP.Lib.T411;
using CommonLib;
using CommonPortableLib;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BezyFB_UWP
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PageT411 : Page
    {
        private Torrent _torrent;
        private static List<Torrent> _movies;
        private static List<SousCategorie> _categories;

        public IMessageDialogService MessageDialog { get; set; }

        public PageT411()
        {
            this.InitializeComponent();
        }
        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await Rafraichir(true);
        }
        private async Task Rafraichir(bool forcer)
        {
            ProgressBarDC.Current.IsProgress = true;
            if (forcer || _movies == null)
            {
                var categorie = comboBox.SelectedItem as SousCategorie;

                if (string.IsNullOrEmpty(textBoxNom.Text))
                {
                    if (rbTop100.IsChecked ?? false)
                    {
                        _movies = await ClientContext.Current.T411.GetTop100();
                    }
                    if (rbTopMonth.IsChecked ?? false)
                    {
                        _movies = await ClientContext.Current.T411.GetTopMonth();
                    }
                    if (rbTopToday.IsChecked ?? false)
                    {
                        _movies = await ClientContext.Current.T411.GetTopToday();
                    }
                    if (rbTopWeek.IsChecked ?? false)
                    {
                        _movies = await ClientContext.Current.T411.GetTopWeek();
                    }
                    if (categorie?.Cat != null && categorie.Cat.Id != -1)
                    {
                        var hashCat = categorie.Cat.Cats.Select(c => c.Value.Id).ToImmutableHashSet();
                        _movies = _movies.Where(t => t.Category == categorie.Cat.Id || hashCat.Contains(t.Category)).ToList();
                    }
                }
                else
                {
                    if (categorie == null || categorie.Cat.Id == -1)
                    {
                        _movies = (await ClientContext.Current.T411.GetQuery(string.Format("{0}", WebUtility.UrlEncode(textBoxNom.Text)))).Torrents;
                    }
                    else
                    {
                        _movies = (await ClientContext.Current.T411.GetQuery(string.Format("{0}", WebUtility.UrlEncode(textBoxNom.Text)),
                                new QueryOptions { CategoryIds = new List<int> { categorie.Cat.Id }, Limit = 1000 })).Torrents;
                    }
                }
            }
            ListView.ItemsSource = _movies.OrderByDescending(t => t.Seeders);
            ProgressBarDC.Current.IsProgress = false;
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {

            var torrent = e.ClickedItem as Torrent;
            _torrent = torrent;
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                // Use "drill in" transition for navigating from master list to detail view
                Frame.Navigate(typeof(TorrentDetailPage), torrent, new DrillInNavigationTransitionInfo());
            }
            else
            {
                // Play a refresh animation when the user switches detail items.
                EnableContentTransitions();
            }
        }

        private async void PageT411_OnLoading(FrameworkElement sender, object args)
        {
            if (null == _categories)
            {
                try
                {
                    _categories = new List<SousCategorie>();
                    foreach (var category1 in (await ClientContext.Current.T411.GetCategory()))
                    {
                        foreach (var cat in category1.Value.Cats)
                        {
                            _categories.Add(new SousCategorie(cat.Value, category1.Value.Name));
                        }
                    }
                    _categories.Insert(0, new SousCategorie(null, "Toutes"));
                }
                catch (Exception e)
                {
                    await ClientContext.Current.MessageDialogService.AfficherMessage(e.Message);
                }

            }
            comboBox.ItemsSource = _categories;

            await Rafraichir(false);
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

            if (isNarrow && oldState == DefaultState && _torrent != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(DetailPage), _torrent, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(ListView, isNarrow);
            if (DetailContentPresenter != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(DetailContentPresenter, !isNarrow);
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

        private async void WebView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_torrent != null)
            {
                ProgressBarDC.Current.IsProgress = true;
                var detail = await ClientContext.Current.T411.GetTorrentDetails(_torrent.Id);
                ((WebView)sender).NavigateToString(detail.Description);
                ProgressBarDC.Current.IsProgress = false;
            }
        }

        private async void WebView_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_torrent != null)
            {
                var detail = await ClientContext.Current.T411.GetTorrentDetails(_torrent.Id);
                ((WebView)sender).NavigateToString(detail.Description);
            }
        }

        private void IsSearch_OnChecked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(SearchState), true);
        }

        private void IsSearch_OnUnChecked(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(NoSearchState), true);
        }
    }
}
