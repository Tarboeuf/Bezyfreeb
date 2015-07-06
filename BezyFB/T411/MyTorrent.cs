using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BezyFB.Annotations;
using BezyFB.BetaSerie;
using BezyFB.Helpers;
using Newtonsoft.Json.Linq;
using T411.Api;

namespace BezyFB.T411
{
    public class MyTorrent : INotifyPropertyChanged
    {
        private readonly Torrent _torrent;
        private double _note;
        private string _nom;
        private OMDb _omDb;

        public MyTorrent(Torrent torrent)
        {
            _torrent = torrent;
        }

        public void Initialiser()
        {
            var task = InitialiserDataAsync();
            task.ContinueWith(t =>
            {
                Nom = t.Result.Item2;
                Note = t.Result.Item1.Note;
                OmDb = t.Result.Item1;
            });
            task.Start();
        }

        private Tuple<OMDb, string> InitialiserData()
        {
            string nom = "";
            try
            {
                nom = GuessIt.GuessNom(_torrent.Name.Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var omDb = new OMDb();
            try
            {
                if (!string.IsNullOrEmpty(nom))
                {
                    omDb = OMDb.GetNote(nom);
                }
                else
                    omDb = OMDb.GetNote(Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new Tuple<OMDb, string>(omDb, nom);
        }

        private Task<Tuple<OMDb, string>> InitialiserDataAsync()
        {
            return new Task<Tuple<OMDb, string>>(InitialiserData);
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}