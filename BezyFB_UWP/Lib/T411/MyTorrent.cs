using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BetaseriesPortableLib;
using CommonLib;

namespace BezyFB_UWP.Lib.T411
{
    public class MyTorrent : INotifyPropertyChanged
    {
        private readonly Torrent _torrent;
        private double _note;
        private string _nom;
        private OMDb _omDb;
        private ApiConnector _apiConnector;
        private IGuessIt GuessIt { get; set; }

        public MyTorrent(Torrent torrent)
        {
            _torrent = torrent;
        }

        public async Task Initialiser()
        {
            if (!string.IsNullOrEmpty(Nom))
            {
            }

            var value = await InitialiserDataAsync(Nom);

            Nom = value.Nom;
            Note = value.OMDB.Note;
            OmDb = value.OMDB;
        }

        private async Task<RetourOMDB> InitialiserData(string nom)
        {
            if (string.IsNullOrEmpty(nom))
            {
                nom = "";
                try
                {
                    nom = await GuessIt.GuessNom(_torrent.Name.Trim());
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            var omDb = new OMDb();
            try
            {
                if (!string.IsNullOrEmpty(nom))
                {
                    omDb = await OMDb.GetNote(nom, _apiConnector);
                }
                else
                {
                    omDb = await OMDb.GetNote(Name, _apiConnector);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return new RetourOMDB { OMDB = omDb, Nom = nom };
        }

        private async Task<RetourOMDB> InitialiserDataAsync(string nom)
        {
            return await Task.Run(() => InitialiserData(nom));
        }

        public Torrent Torrent => _torrent;

        public string Name => _torrent.Name;

        public long Size => _torrent.Size;

        public int Seeders => _torrent.Seeders;

        public int TimesCompleted => _torrent.Times_completed;

        public double TimesCompletedByHours => _torrent.Times_completed / (DateTime.Now - _torrent.Added).TotalHours;

        public double Note
        {
            get { return _note; }
            set
            {
                _note = value;
                OnPropertyChanged();
            }
        }

        public string Nom
        {
            get { return _nom; }
            set
            {
                _nom = value;
                OnPropertyChanged();
            }
        }

        public OMDb OmDb
        {
            get { return _omDb; }
            set
            {
                _omDb = value;
                OnPropertyChanged();
            }
        }

        //g Name}" />
        //nding Size, Converte
        //inding Seeders}" />
        //ng Times_completed}"

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class RetourOMDB
        {
            public string Nom { get; set; }
            public OMDb OMDB { get; set; }
        }
    }
}