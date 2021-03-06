﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BezyFB.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BezyFB.Annotations;
using System.Collections.ObjectModel;
using BetaseriesStandardLib;
using FreeboxStandardLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour FreeboxUserControl.xaml
    /// </summary>
    public partial class FreeboxUserControl : MyUserControl
    {
        public FreeboxUserControl()
        {
            InitializeComponent();
        }

        private void FreeSpace_OnClick(object sender, RoutedEventArgs e)
        {
            TailleDossierFreebox window = new TailleDossierFreebox();
            window.ShowDialog();
        }


        private async void SupprimerFilm_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = ((Button)sender).DataContext as OMDb;
            if (null != dc)
            {
                await ClientContext.Current.Freebox.DeleteFile(Settings.Default.PathFilm + dc.FileName);
            }
        }

        private async void DeleteTerminated_OnClick(object sender, RoutedEventArgs e)
        {
            var user = DataContext as UserFreeboxVM;
            if (null != user)
            {
                foreach (var downloadItem in user.Downloads)
                {
                    if (downloadItem.Status == "done")
                    {
                        await ClientContext.Current.Freebox.DeleteTerminated(downloadItem.Id);
                    }
                }
            }
            await Refresh();
        }

        private async void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        public async Task Refresh(bool forcer = true)
        {
            if (forcer || !(DataContext is UserFreeboxVM))
            {
                var dc = new UserFreeboxVM(await ClientContext.Current.Freebox.GetInfosFreebox());
                DataContext = dc;
                foreach (var item in dc.Downloads)
                {
                    await item.LoadImage();
                }
            }
            else
            {
                var dc = DataContext as UserFreeboxVM;
                dc.Update(await ClientContext.Current.Freebox.GetInfosFreebox());

                foreach (var item in dc.Downloads)
                {
                    if (string.IsNullOrEmpty(item.ImagePath))
                    {
                        await item.LoadImage();
                    }
                }
            }
        }

        private async void DeplacerTelechargementFini_OnClick(object sender, RoutedEventArgs e)
        {
            await ClientContext.Current.Freebox.DeplacerTelechargementFini();
        }

        private async void DeleteEmptyFolder_OnClick(object sender, RoutedEventArgs e)
        {
            await ClientContext.Current.Freebox.DeleteEmptyFolder();
        }
    }

    public class UserFreeboxVM : INotifyPropertyChanged
    {

        public UserFreeboxVM(UserFreebox fb)
        {
            Downloads = new ObservableCollection<DownloadItemVM>(fb.Downloads.Select(d => new DownloadItemVM(d)));
            FreeSpace = fb.FreeSpace;
            Ratio = fb.Ratio;
            PathFilm = fb.PathFilm;
        }

        public long FreeSpace { get; set; }
        public double Ratio { get; set; }
        public ObservableCollection<DownloadItemVM> Downloads { get; set; }
        public string PathFilm { get; private set; }

        //public ObservableCollection<OMDb> Movies { get; set; }
        //public async void LoadMovies()
        //{
        //    foreach (var item in await _fb.Ls(PathFilm, false))
        //    {
        //        var nom = await GuessIt.GuessNom(item);
        //        var omdb = await OMDb.GetNote(nom, item);
        //        Movies.Add(omdb);
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Update(UserFreebox userFreebox)
        {
            foreach (var item in Downloads.ToList())
            {
                var freeboxItem = userFreebox.Downloads.FirstOrDefault(d => d.Id == item.Id);
                if (null != freeboxItem)
                {
                    item.Update(freeboxItem);
                }
                else
                {
                    Downloads.Remove(item);
                }
            }
            foreach (var item in userFreebox.Downloads)
            {
                if(!Downloads.Any(d => d.Id == item.Id))
                {
                    Downloads.Add(new DownloadItemVM(item));
                }
            }
        }
    }


    public class DownloadItemVM : INotifyPropertyChanged
    {
        public DownloadItemVM(DownloadItem item)
        {
            Name = item.Name;
            Status = item.Status;
            Pourcentage = item.Pourcentage;
            RxPourcentage = item.RxPourcentage;
            Id = item.Id;
        }

        public async Task LoadImage()
        {
            var guess = await ClientContext.Current.GuessIt.GuessNom(Name + ".mkv");
            if (!string.IsNullOrEmpty(guess))
                NomPropre = guess;
            else
            {
                NomPropre = Name;
            }
            var note = await OMDb.GetNote(guess, ClientContext.Current.ApiConnector);

            ImagePath = note?.Poster;
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public double Pourcentage { get; set; }
        public double RxPourcentage { get; set; }
        public int Id { get; set; }

        public string NomPropre
        {
            get { return _nomPropre ?? Name; }
            set
            {
                _nomPropre = value;
                RaisePropertyChanged(nameof(NomPropre));
            }
        }


        private string _ImagePath;
        private string _nomPropre;

        public string ImagePath
        {
            get { return _ImagePath; }
            set
            {
                _ImagePath = value;
                RaisePropertyChanged(nameof(ImagePath));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void Update(DownloadItem freeboxItem)
        {
            Status = freeboxItem.Status;
            Pourcentage = freeboxItem.Pourcentage;
            RxPourcentage = freeboxItem.RxPourcentage;
        }
    }
}
