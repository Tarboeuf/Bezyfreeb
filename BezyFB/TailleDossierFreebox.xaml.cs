using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using BezyFB.Annotations;
using FreeboxStandardLib;

namespace BezyFB
{
    /// <summary>
    /// Logique d'interaction pour TailleDossierFreebox.xaml
    /// </summary>
    public partial class TailleDossierFreebox : Window, INotifyPropertyChanged
    {
        private Freebox _freebox;
        private ObservableCollection<Fichier> _fichiers;

        public TailleDossierFreebox()
        {
            _freebox = ClientContext.Current.Freebox;
            InitializeComponent();

            Fichiers = new ObservableCollection<Fichier>();
            
            DataContext = this;
        }

        private async Task<ObservableCollection<Fichier>> Charger(string directory, Fichier parent)
        {
            ObservableCollection<Fichier> fichiers = new ObservableCollection<Fichier>();
            try
            {
                var files = await _freebox.LsFileInfo(directory);

                if (files == null) return null;
                foreach (var file in files.Where(f => !f.Name.StartsWith(".")))
                {
                    var fichier = new Fichier(parent)
                    {
                        Nom = file.Name,
                        Taille = long.Parse(file.Size),
                        IsDossier = file.Type == "dir"
                    };
                    fichiers.Add(fichier);
                    if (file.Type == "dir")
                    {
                        var sousFichiers = await Charger(directory + "/" + file.Name, fichier);
                        fichier.Fichiers = sousFichiers;
                    }
                    var view = CollectionViewSource.GetDefaultView(fichiers);
                    view.SortDescriptions.Add(new SortDescription("TailleTotal", ListSortDirection.Descending));
                }
                
            }
            catch (Exception)
            {
                throw;
            }
            return fichiers;
        }

        public ObservableCollection<Fichier> Fichiers
        {
            get { return _fichiers; }
            set
            {
                if (Equals(value, _fichiers)) return;
                _fichiers = value;
                OnPropertyChanged();
            }
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Fichiers = await Charger("/", null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Fichier
    {
        private readonly Fichier _parent;

        public Fichier(Fichier parent)
        {
            _parent = parent;
            Fichiers = new ObservableCollection<Fichier>();
        }

        public string Nom { get; set; }
        public long Taille { get; set; }

        public long TailleTotal
        {
            get
            {
                if (IsDossier && Fichiers != null)
                    return Fichiers.Select(f => f.TailleTotal).Sum();
                return Taille / (1024 * 1024);
            }
        }

        public double Pourcentage
        {
            get
            {
                if (_parent == null)
                    return 1d;
                return TailleTotal / (double)_parent.TailleTotal;
            }
        }

        public bool IsDossier { get; set; }

        public ObservableCollection<Fichier> Fichiers { get; set; }

    }
}
