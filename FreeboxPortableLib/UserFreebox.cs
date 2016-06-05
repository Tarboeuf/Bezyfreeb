using System.Collections.Generic;

namespace FreeboxPortableLib
{
    public class UserFreebox
    {
        private readonly Freebox _fb;

        public UserFreebox(Freebox fb)
        {
            _fb = fb;
            Downloads = new List<DownloadItem>();
            //Movies = new ObservableCollection<OMDb>();
        }

        public long FreeSpace { get; set; }
        public double Ratio { get; set; }
        public List<DownloadItem> Downloads { get; set; }

        //public ObservableCollection<OMDb> Movies { get; set; }
        public string PathFilm { get; private set; }

        //public async void LoadMovies()
        //{
        //    foreach (var item in await _fb.Ls(PathFilm, false))
        //    {
        //        var nom = await GuessIt.GuessNom(item);
        //        var omdb = await OMDb.GetNote(nom, item);
        //        Movies.Add(omdb);
        //    }
        //}
    }
}