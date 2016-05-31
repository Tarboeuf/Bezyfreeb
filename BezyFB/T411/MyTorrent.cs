using BezyFB.BetaSerieLib;
using CommonPortableLib;
using Essy.Tools.InputBox;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BezyFB.T411
{
    public class MyTorrent : INotifyPropertyChanged
    {
        private readonly Torrent _torrent;
        private double _note;
        private string _nom;
        private OMDb _omDb;
        private GuessIt _guessIt;
        private IApiConnectorService _apiConnetor;

        public MyTorrent(Torrent torrent, GuessIt guessIt, IApiConnectorService apiConnetor)
        {
            _apiConnetor = apiConnetor;
            _guessIt = guessIt;
            _torrent = torrent;
        }

        public async Task Initialiser()
        {
            if (!string.IsNullOrEmpty(Nom))
            {
                Nom = InputBox.ShowInputBox("Quel est le nom du film ?", Nom, false);
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
                    nom = await _guessIt.GuessNom(_torrent.Name.Trim());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            var omDb = new OMDb();
            try
            {
                if (!string.IsNullOrEmpty(nom))
                {
                    omDb = await OMDb.GetNote(nom, _apiConnetor);
                }
                else
                    omDb = await OMDb.GetNote(Name, _apiConnetor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new RetourOMDB { OMDB = omDb, Nom = nom };
        }

        private async Task<RetourOMDB> InitialiserDataAsync(string nom)
        {
            return await Task.Run(() => InitialiserData(nom));
        }

        public Torrent Torrent { get { return _torrent; } }

        public string Name { get { return _torrent.Name; } }

        public long Size { get { return _torrent.Size; } }

        public int Seeders { get { return _torrent.Seeders; } }

        public int Times_completed { get { return _torrent.Times_completed; } }

        public double TimesCompletedByHours { get { return _torrent.Times_completed / (DateTime.Now - _torrent.Added).TotalHours; } }

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
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private class RetourOMDB
        {
            public string Nom { get; set; }
            public OMDb OMDB { get; set; }
        }
    }
}