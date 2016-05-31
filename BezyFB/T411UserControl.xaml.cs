using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BezyFB.Properties;
using BezyFB.T411;
using Microsoft.Win32;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour T411UserControl.xaml
    /// </summary>
    public partial class T411UserControl
    {
        public T411UserControl()
        {
            InitializeComponent();
        }

        private async void buttonT411Rechercher_Click(object sender, RoutedEventArgs e)
        {
            Window.pb.Visibility = Visibility.Visible;
            List<Torrent> items = null;
            if (string.IsNullOrEmpty(textBoxRechercheT411.Text))
            {
                if (GetTop100.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top 100");
                    items = (await ClientContext.Current.T411.GetTop100());
                }
                else if (GetTopMonth.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top month");
                    items = (await ClientContext.Current.T411.GetTopMonth());
                }
                else if (GetTopWeek.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top week");
                    items = (await ClientContext.Current.T411.GetTopWeek());
                }
                else if (GetTopToday.IsChecked == true)
                {
                    SetStatusText("Connecté t411 recherche top today");
                    items = (await ClientContext.Current.T411.GetTopToday());
                }
                if (AvecCategorie.IsChecked == true)
                    items =
                        items.Where(t => t.CategoryName == ((SousCategorie) comboCategoryT411.SelectedValue).Cat.Name)
                            .ToList();
            }
            else
            {
                SetStatusText("Connecté t411 recherche par nom");
                if (AvecCategorie.IsChecked == true)
                {
                    items =
                        (await
                            ClientContext.Current.T411.GetQuery(
                                string.Format("{0}", HttpUtility.UrlEncode(textBoxRechercheT411.Text)),
                                new QueryOptions
                                {
                                    CategoryIds =
                                        new List<int> {((SousCategorie) comboCategoryT411.SelectedValue).Cat.Id},
                                    Limit = 1000
                                })).Torrents;
                }
                else
                {
                    items =
                        (await
                            ClientContext.Current.T411.GetQuery(string.Format("{0}",
                                HttpUtility.UrlEncode(textBoxRechercheT411.Text)))).Torrents;
                }
            }

            lv.ItemsSource =
                items.OrderByDescending(t => t.Times_completed)
                    .Select(t => new MyTorrent(t, ClientContext.Current.GuessIt, ClientContext.Current.ApiConnector));
            SetStatusText("torrents récupéré");
            Window.pb.Visibility = Visibility.Collapsed;
        }


        public async Task LoadT411()
        {
            var worker = new BackgroundWorker();
            Window.pb.Visibility = Visibility.Visible;
            try
            {
                var topWeek = await ClientContext.Current.T411.GetTopWeek();
                await Dispatcher.BeginInvoke((Action) (() => lv.ItemsSource =
                    topWeek
                        .OrderByDescending(t => t.Times_completed)
                        .Select(t => new MyTorrent(t, ClientContext.Current.GuessIt, ClientContext.Current.ApiConnector))));

                var categories = new List<SousCategorie>();
                foreach (var category1 in (await ClientContext.Current.T411.GetCategory()))
                {
                    foreach (var cat in category1.Value.Cats)
                    {
                        categories.Add(new SousCategorie(cat.Value, category1.Value.Name));
                    }
                }

                await Dispatcher.BeginInvoke((Action) (() =>
                {
                    comboCategoryT411.ItemsSource = categories;
                    comboCategoryT411.SelectedIndex =
                        categories.IndexOf(categories.FirstOrDefault(c => c.Cat.Name == "Film"));
                }));

                var user = await ClientContext.Current.T411.GetUserDetails(ClientContext.Current.T411.UserId);
                await Dispatcher.BeginInvoke((Action) (() =>
                    labelT411.Content =
                        user.Username + " Ratio : " + (user.Uploaded/(double) user.Downloaded).ToString("##.###")));

                Window.gridButton.Visibility = Visibility.Collapsed;
                Window.pb.Visibility = Visibility.Collapsed;

                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                await
                    ClientContext.Current.MessageDialogService.AfficherMessage(
                        "Impossible de charger les données de T411.\r\n" + ex.Message);
            }
        }
        private async void ButtonTelechargerTorrent_OnClick(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    using (var stream = ClientContext.Current.T411.DownloadTorrent(torrent.Torrent.Id))
                    {
                        if (string.IsNullOrEmpty(Settings.Default.TokenFreebox))
                        {
                            var sfd = new SaveFileDialog();
                            sfd.FileName = torrent.Name + ".torrent";
                            if (sfd.ShowDialog() ?? false)
                            {
                                using (var s = File.Create(sfd.FileName))
                                {
                                    stream.CopyTo(s);
                                }
                            }
                        }
                        else
                        {
                            await ClientContext.Current.Freebox.DownloadFile(stream, torrent.Name + ".torrent", Settings.Default.PathFilm, false);
                        }
                    }
                }
            }
        }

        private async void ButtonExtraInfoOnClick(object sender, RoutedEventArgs e)
        {
            Window.pb.Visibility = Visibility.Visible;
            Button senderButton = sender as Button;
            if (null != senderButton)
            {
                var torrent = senderButton.Tag as MyTorrent;
                if (null != torrent)
                {
                    await torrent.Initialiser();
                }
            }
            SetStatusText("Extra infos récupérées");
            Window.pb.Visibility = Visibility.Collapsed;
        }

    }
}